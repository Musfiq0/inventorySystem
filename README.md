# Inventory Management System

A simple web-based inventory management system built with ASP.NET Core MVC and MySQL.

## Quick Start

### Requirements
- **XAMPP** with MySQL running
- **.NET 8 SDK** installed

### Run the Application
```bash
dotnet run
```
Open http://localhost:5000 in your browser.

## Features

-  **Create inventories** (storage locations)
-  **Add items** to inventories with details like price, quantity, SKU
-  **Search and filter** items and inventories
-  **Edit and delete** inventories and items
-  **Public access** - no login required
-  **Responsive design** - works on mobile devices

## Project Structure

```
InventoryManagement/
 Program.cs              # Application startup
 appsettings.json        # Database configuration
 Models/                 # Data models
    Inventory.cs        # Inventory model
    Item.cs            # Item model
    AppDbContext.cs    # Database context
    SeedData.cs        # Sample data
 Controllers/            # Request handlers
    InventoryController.cs
    ItemController.cs
 Views/                  # HTML templates
    Inventory/         # Inventory pages
    Item/              # Item pages
    Shared/            # Common layouts
 wwwroot/               # Static files (CSS, JS)
 Migrations/            # Database schema
```

## Database

The application automatically creates a MySQL database named `InventoryManagement` with sample data:
- 3 sample inventories (Office Supplies, Electronics, Warehouse Storage)
- 10 sample items with various quantities and prices

## Configuration

Update `appsettings.json` to change database settings:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=InventoryManagement;User=root;Password=;Port=3306;"
  }
}
```

## Troubleshooting

**Database Connection Issues:**
1. Ensure XAMPP MySQL is running (green status)
2. Check connection string in `appsettings.json`
3. Restart the application

**Build Errors:**
```bash
dotnet clean
dotnet build
```

**Port Already in Use:**
```bash
dotnet run --urls http://localhost:5000
```
