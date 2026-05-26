# 📚 Cross-Platform Enterprise Book Inventory System

A premium, modern desktop database application built using **C# (.NET 8.0)** and **Avalonia UI**, interacting with a local **MySQL** database.

---

## 🚀 The Linux & C# Cross-Platform Flex
Traditionally, desktop GUI development in C# (using WinForms or WPF) has been strictly bound to the Windows operating system. 

This project challenges that legacy by demonstrating a **fully native C# Desktop Application developed, compiled, and executed entirely on a Linux environment (Ubuntu/Debian)**. By leveraging the advanced **Avalonia UI** framework and the modern cross-platform **.NET 8.0 SDK**, this application achieves true platform independence with high-tier graphics rendering, native OS performance, and beautiful modern aesthetics on Linux desktops.

---

## ✨ Premium Features

*   **Linux Native Execution:** Built and compiled on Linux targeting the native X11 windowing subsystem.
*   **Aesthetic Slate Dark Mode:** A stunning, premium dark dashboard utilizing custom floating input panels, glassmorphic card shadows, and harmonized HSL color tokens.
*   **Self-Bootstrapping Database:** Features a zero-configuration database bootstrapper. On startup, the application connects to your MySQL server and automatically creates the target database (`BookDB`) and table schema (`Books`) if they are missing.
*   **Security-First Architecture:** Passwords and credentials are never hardcoded in the source code. The project utilizes dynamic JSON loading from a secure local `appsettings.json` file which is completely excluded from version control (`.gitignore` protected).
*   **Sleek Multi-Column DataGrid:** Instantly displays stored inventories with support for custom data bindings, row selection events, and automated text inputs population.
*   **Robust Data Queries:** Implements secure, parameterized SQL queries to prevent SQL Injection attacks on all operations (Insert, Update, Delete).

---

## 🛠 Tech Stack

*   **Runtime environment:** .NET 8.0 SDK
*   **Language:** C# 12
*   **GUI Framework:** Avalonia UI (v11.0)
*   **Database Engine:** MySQL Server (Local Instance)
*   **Driver:** MySql.Data (v9.7)

---

## 📦 Setting Up the Environment

### 1. Prerequisites (Ubuntu/Debian Linux)
Make sure you have the .NET 8.0 SDK and MySQL server installed on your system.

```bash
# Install .NET 8 SDK
sudo apt update && sudo apt install -y dotnet-sdk-8.0

# Install MySQL Server (if not already installed)
sudo apt install -y mysql-server
```

### 2. Local Configuration
To run this application, you must provide your local database credentials.

1.  Copy the example template file to create a local settings file:
    ```bash
    cp appsettings.json.example appsettings.json
    ```
2.  Open `appsettings.json` and replace `YOUR_DB_USER` and `YOUR_DB_PASSWORD` with your local MySQL credentials:
    ```json
    {
      "ConnectionString": "server=localhost;database=BookDB;uid=usman;pwd=BookManager@2026!;AllowPublicKeyRetrieval=True;SslMode=Disabled;"
    }
    ```

---

## 🏁 Running the Application

Since the application features a **Self-Bootstrapping Database setup**, you do not need to import any SQL files or create databases manually! 

Simply navigate to the project directory and execute the run command:

```bash
# Clean up any old temporary caches and run
dotnet build
dotnet run
```

On execution:
1.  The app connects to your MySQL server using your `appsettings.json` configuration.
2.  It automatically creates `BookDB` and the `Books` table if they do not exist.
3.  The premium Dark-Mode Dashboard will launch instantly, displaying the fully populated inventories.
