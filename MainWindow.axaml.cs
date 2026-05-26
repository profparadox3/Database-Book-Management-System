using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace BookGuiSystem
{
    // =========================================================================
    // DATA MODEL CLASS FOR BOOK RECORDS (MATCHES YOUR Books TABLE)
    // =========================================================================
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public decimal Price { get; set; }
        public string Genre { get; set; } = "";
    }

    // =========================================================================
    // MAIN APPLICATION WINDOW BACK-END IMPLEMENTATION
    // =========================================================================
    public partial class MainWindow : Window
    {
        // Connection string loaded dynamically from local appsettings.json for security
        private readonly string connString;

        public MainWindow()
        {
            connString = LoadConnectionString();
            EnsureDatabaseSetup(); // Automatically bootstraps BookDB and Books table if they don't exist
            InitializeComponent();
            WireUpEvents();
            LoadData(); // Automatically loads existing records on startup
        }

        /// <summary>
        /// Automatically bootstraps the database and table if they do not exist on the host system.
        /// </summary>
        private void EnsureDatabaseSetup()
        {
            // Connect to MySQL server directly (without selecting BookDB yet)
            string masterConnString = connString.Replace("database=BookDB;", "");
            
            using (MySqlConnection conn = new MySqlConnection(masterConnString))
            {
                try
                {
                    conn.Open();
                    
                    // Create database if not exists
                    using (MySqlCommand cmd = new MySqlCommand("CREATE DATABASE IF NOT EXISTS BookDB;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create table if not exists
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS BookDB.Books (
                            book_id INT AUTO_INCREMENT PRIMARY KEY,
                            book_title VARCHAR(100) NOT NULL,
                            author VARCHAR(50),
                            price DECIMAL(10,2),
                            genre VARCHAR(30)
                        );";
                    using (MySqlCommand cmd = new MySqlCommand(createTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception)
                {
                    // Fail silently, LoadData() will catch and show the database connection errors on UI
                }
            }
        }

        /// <summary>
        /// Safely loads database credentials from local config file, falling back securely if not found.
        /// </summary>
        private static string LoadConnectionString()
        {
            string configPath = "appsettings.json";
            if (System.IO.File.Exists(configPath))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(configPath);
                    using (var doc = System.Text.Json.JsonDocument.Parse(json))
                    {
                        if (doc.RootElement.TryGetProperty("ConnectionString", out var prop))
                        {
                            return prop.GetString() ?? "";
                        }
                    }
                }
                catch { }
            }
            return "server=localhost;database=BookDB;uid=YOUR_DB_USER;pwd=YOUR_DB_PASSWORD;AllowPublicKeyRetrieval=True;SslMode=Disabled;";
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Dynamically retrieves control references by name and hooks up events.
        /// </summary>
        private void WireUpEvents()
        {
            var btnSave = this.FindControl<Button>("btnSave");
            var btnEdit = this.FindControl<Button>("btnEdit");
            var btnDelete = this.FindControl<Button>("btnDelete");
            var btnDisplay = this.FindControl<Button>("btnDisplay");
            var dgBooks = this.FindControl<DataGrid>("dgBooks");

            if (btnSave != null) btnSave.Click += OnSaveClick;
            if (btnEdit != null) btnEdit.Click += OnEditClick;
            if (btnDelete != null) btnDelete.Click += OnDeleteClick;
            if (btnDisplay != null) btnDisplay.Click += OnDisplayClick;
            if (dgBooks != null) dgBooks.SelectionChanged += OnGridSelectionChanged;
        }

        // =========================================================================
        // READ: SELECT ALL RECORDS FROM YOUR 'Books' TABLE
        // =========================================================================
        private void LoadData()
        {
            var dgBooks = this.FindControl<DataGrid>("dgBooks");
            var lblStatus = this.FindControl<TextBlock>("lblStatus");
            var booksList = new List<Book>();

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    // Selecting fields matching your exact columns: book_id, book_title, author, price, genre
                    string query = "SELECT book_id, book_title, author, price, genre FROM Books";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    
                    conn.Open();
                    
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            booksList.Add(new Book
                            {
                                Id = Convert.ToInt32(reader["book_id"]),
                                Title = reader["book_title"]?.ToString() ?? "",
                                Author = reader["author"]?.ToString() ?? "",
                                Price = reader["price"] == DBNull.Value ? 0.00m : Convert.ToDecimal(reader["price"]),
                                Genre = reader["genre"]?.ToString() ?? ""
                            });
                        }
                    }

                    if (dgBooks != null)
                    {
                        dgBooks.ItemsSource = booksList;
                    }

                    if (lblStatus != null)
                    {
                        lblStatus.Text = "System Status: Connected and fully refreshed!";
                    }
                }
                catch (Exception ex)
                {
                    if (lblStatus != null)
                    {
                        lblStatus.Text = $"Database Error: {ex.Message}";
                    }
                }
            }
        }

        // =========================================================================
        // CREATE: INSERT INTO 'Books' TABLE
        // =========================================================================
        public void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            var txtBookName = this.FindControl<TextBox>("txtBookName");
            var txtAuthor = this.FindControl<TextBox>("txtAuthor");
            var txtPrice = this.FindControl<TextBox>("txtPrice");
            var txtGenre = this.FindControl<TextBox>("txtGenre");
            var lblStatus = this.FindControl<TextBlock>("lblStatus");

            // Validation: Title is required
            if (txtBookName == null || string.IsNullOrWhiteSpace(txtBookName.Text))
            {
                if (lblStatus != null) lblStatus.Text = "Validation Error: Book Title is required!";
                return;
            }

            // Parse Price safely
            decimal priceValue = 0.00m;
            if (txtPrice != null && !string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                if (!decimal.TryParse(txtPrice.Text.Trim(), out priceValue))
                {
                    if (lblStatus != null) lblStatus.Text = "Validation Error: Enter a valid numeric price (e.g. 19.99)!";
                    return;
                }
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    // Parameterized query using your column names: book_title, author, price, genre
                    string query = "INSERT INTO Books (book_title, author, price, genre) VALUES (@title, @author, @price, @genre)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    
                    cmd.Parameters.AddWithValue("@title", txtBookName.Text.Trim());
                    cmd.Parameters.AddWithValue("@author", txtAuthor?.Text?.Trim() ?? "");
                    cmd.Parameters.AddWithValue("@price", priceValue);
                    cmd.Parameters.AddWithValue("@genre", txtGenre?.Text?.Trim() ?? "");

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    if (lblStatus != null) lblStatus.Text = "Success: Book record saved successfully!";
                    
                    LoadData();
                    ClearInputs();
                }
                catch (Exception ex)
                {
                    if (lblStatus != null) lblStatus.Text = $"Insert Error: {ex.Message}";
                }
            }
        }

        // =========================================================================
        // UPDATE: EDIT 'Books' RECORD TARGETING UNIQUE 'book_id'
        // =========================================================================
        public void OnEditClick(object? sender, RoutedEventArgs e)
        {
            var txtBookId = this.FindControl<TextBox>("txtBookId");
            var txtBookName = this.FindControl<TextBox>("txtBookName");
            var txtAuthor = this.FindControl<TextBox>("txtAuthor");
            var txtPrice = this.FindControl<TextBox>("txtPrice");
            var txtGenre = this.FindControl<TextBox>("txtGenre");
            var lblStatus = this.FindControl<TextBlock>("lblStatus");

            if (txtBookId == null || string.IsNullOrWhiteSpace(txtBookId.Text) || !int.TryParse(txtBookId.Text, out int bookId))
            {
                if (lblStatus != null) lblStatus.Text = "Validation Error: Select a book from the table first!";
                return;
            }

            if (txtBookName == null || string.IsNullOrWhiteSpace(txtBookName.Text))
            {
                if (lblStatus != null) lblStatus.Text = "Validation Error: Book Title cannot be empty!";
                return;
            }

            decimal priceValue = 0.00m;
            if (txtPrice != null && !string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                if (!decimal.TryParse(txtPrice.Text.Trim(), out priceValue))
                {
                    if (lblStatus != null) lblStatus.Text = "Validation Error: Enter a valid numeric price!";
                    return;
                }
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    string query = "UPDATE Books SET book_title=@title, author=@author, price=@price, genre=@genre WHERE book_id=@id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    
                    cmd.Parameters.AddWithValue("@id", bookId);
                    cmd.Parameters.AddWithValue("@title", txtBookName.Text.Trim());
                    cmd.Parameters.AddWithValue("@author", txtAuthor?.Text?.Trim() ?? "");
                    cmd.Parameters.AddWithValue("@price", priceValue);
                    cmd.Parameters.AddWithValue("@genre", txtGenre?.Text?.Trim() ?? "");

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    if (lblStatus != null) lblStatus.Text = $"Success: Book ID {bookId} has been updated!";
                    
                    LoadData();
                    ClearInputs();
                }
                catch (Exception ex)
                {
                    if (lblStatus != null) lblStatus.Text = $"Update Error: {ex.Message}";
                }
            }
        }

        // =========================================================================
        // DELETE: REMOVE RECORD TARGETING 'book_id'
        // =========================================================================
        public void OnDeleteClick(object? sender, RoutedEventArgs e)
        {
            var txtBookId = this.FindControl<TextBox>("txtBookId");
            var lblStatus = this.FindControl<TextBlock>("lblStatus");

            if (txtBookId == null || string.IsNullOrWhiteSpace(txtBookId.Text) || !int.TryParse(txtBookId.Text, out int bookId))
            {
                if (lblStatus != null) lblStatus.Text = "Validation Error: Select a book from the table first!";
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    string query = "DELETE FROM Books WHERE book_id=@id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    
                    cmd.Parameters.AddWithValue("@id", bookId);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    if (lblStatus != null) lblStatus.Text = $"Success: Book ID {bookId} deleted successfully!";
                    
                    LoadData();
                    ClearInputs();
                }
                catch (Exception ex)
                {
                    if (lblStatus != null) lblStatus.Text = $"Delete Error: {ex.Message}";
                }
            }
        }

        // =========================================================================
        // REFRESH: MANUALLY TRIGGER LOAD
        // =========================================================================
        public void OnDisplayClick(object? sender, RoutedEventArgs e)
        {
            LoadData();
        }

        // =========================================================================
        // SELECTION EVENT: AUTO-POPULATE TEXT BOXES
        // =========================================================================
        private void OnGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var dgBooks = this.FindControl<DataGrid>("dgBooks");
            var txtBookId = this.FindControl<TextBox>("txtBookId");
            var txtBookName = this.FindControl<TextBox>("txtBookName");
            var txtAuthor = this.FindControl<TextBox>("txtAuthor");
            var txtPrice = this.FindControl<TextBox>("txtPrice");
            var txtGenre = this.FindControl<TextBox>("txtGenre");
            var lblStatus = this.FindControl<TextBlock>("lblStatus");

            if (dgBooks != null && dgBooks.SelectedItem is Book selectedBook)
            {
                if (txtBookId != null) txtBookId.Text = selectedBook.Id.ToString();
                if (txtBookName != null) txtBookName.Text = selectedBook.Title;
                if (txtAuthor != null) txtAuthor.Text = selectedBook.Author;
                if (txtPrice != null) txtPrice.Text = selectedBook.Price.ToString("F2");
                if (txtGenre != null) txtGenre.Text = selectedBook.Genre;
                
                if (lblStatus != null)
                {
                    lblStatus.Text = $"Selected Book Identity ID: {selectedBook.Id}";
                }
            }
        }

        // =========================================================================
        // HELPERS: CLEAR ALL INPUT FIELDS
        // =========================================================================
        private void ClearInputs()
        {
            var txtBookId = this.FindControl<TextBox>("txtBookId");
            var txtBookName = this.FindControl<TextBox>("txtBookName");
            var txtAuthor = this.FindControl<TextBox>("txtAuthor");
            var txtPrice = this.FindControl<TextBox>("txtPrice");
            var txtGenre = this.FindControl<TextBox>("txtGenre");

            if (txtBookId != null) txtBookId.Text = "";
            if (txtBookName != null) txtBookName.Text = "";
            if (txtAuthor != null) txtAuthor.Text = "";
            if (txtPrice != null) txtPrice.Text = "";
            if (txtGenre != null) txtGenre.Text = "";
        }
    }
}