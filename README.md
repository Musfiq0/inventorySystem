# Inventory Management System

A complete inventory management system with user authentication and admin controls.

## Run Locally

### Prerequisites
- XAMPP (for MySQL database)
- .NET 8.0 SDK

### Setup
1. Start XAMPP and enable MySQL
2. Clone the repository
3. Run the application:
   ```
   dotnet run
   ```
4. Open http://localhost:5000
5. Login with admin: `admin@inventory.com` / `Admin123!`

## Live Deployment

### Render.com
1. Connect your GitHub repository to Render
2. Create a new Blueprint deployment
3. Update `render.yaml` with your repository URL
4. Deploy automatically with MySQL database

Admin features allow editing all content directly from the website.

## Database

The application automatically:
- Creates MySQL database schema using Entity Framework migrations
- Seeds initial data including admin user and sample inventories
- Supports both development (XAMPP) and production (Render) MySQL databases

## Configuration

### Development (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Database=InventoryManagement;User=root;Password=;Port=3306;AllowUserVariables=true;Convert Zero Datetime=True;"
  }
}
```

### Production
Uses environment variables:
- `DATABASE_URL` for database connection (provided by Render)

## User Roles

**Admin Users:**
- Full access to all features
- Can edit site content dynamically
- User management capabilities
- Complete inventory control

**Regular Users:**
- View inventories and items
- Manage inventory items only
- Cannot edit site content or manage users

## Troubleshooting

**Database Connection Issues:**
1. Ensure XAMPP MySQL is running (development)
2. Verify DATABASE_URL environment variable (production)
3. Check database server accessibility

**Docker Issues:**
```bash
docker logs container_name
```

**Build Errors:**
```bash
dotnet clean
dotnet restore
dotnet build
```
