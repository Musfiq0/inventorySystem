using Microsoft.EntityFrameworkCore;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Initial connection string: {connectionString ?? "Not set"}");

if (builder.Environment.IsProduction())
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    Console.WriteLine($"DATABASE_URL: {(string.IsNullOrEmpty(databaseUrl) ? "Not set" : "Set (length: " + databaseUrl.Length + ")")}");
    
    // Try multiple environment variables that Render might use
    var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL");
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    var dbName = Environment.GetEnvironmentVariable("DB_NAME");
    var dbUser = Environment.GetEnvironmentVariable("DB_USER");
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"; // Default to PostgreSQL port
    
    // Try PostgreSQL-specific environment variables that Render might use
    var pgHost = Environment.GetEnvironmentVariable("PGHOST");
    var pgPort = Environment.GetEnvironmentVariable("PGPORT");
    var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
    var pgUser = Environment.GetEnvironmentVariable("PGUSER");
    var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
    
    Console.WriteLine($"Alternative vars - MYSQL_URL: {!string.IsNullOrEmpty(mysqlUrl)}, DB_HOST: {!string.IsNullOrEmpty(dbHost)}");
    Console.WriteLine($"PostgreSQL vars - PGHOST: {!string.IsNullOrEmpty(pgHost)}, PGDATABASE: {!string.IsNullOrEmpty(pgDatabase)}, PGUSER: {!string.IsNullOrEmpty(pgUser)}");
    
    if (!string.IsNullOrWhiteSpace(databaseUrl) && databaseUrl.Trim().Length > 0)
    {
        try
        {
            var trimmedUrl = databaseUrl.Trim();
            Console.WriteLine($"Attempting to parse DATABASE_URL starting with: {trimmedUrl.Substring(0, Math.Min(15, trimmedUrl.Length))}...");
            
            // Handle different potential formats from Render
            if (trimmedUrl.StartsWith("postgresql://") || trimmedUrl.StartsWith("postgres://"))
            {
                // Parse DATABASE_URL format: postgresql://username:password@host:port/database
                var uri = new Uri(trimmedUrl);
                
                // Validate URI components
                if (uri.Host != null && uri.UserInfo != null && uri.UserInfo.Contains(':'))
                {
                    var userParts = uri.UserInfo.Split(':');
                    if (userParts.Length == 2)
                    {
                        var uriPort = uri.Port > 0 ? uri.Port : 5432; // Default PostgreSQL port
                        connectionString = $"Host={uri.Host};Port={uriPort};Database={uri.AbsolutePath.TrimStart('/')};Username={userParts[0]};Password={userParts[1]};SSL Mode=Require;Trust Server Certificate=true;";
                        Console.WriteLine("Successfully parsed DATABASE_URL for PostgreSQL");
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Invalid DATABASE_URL format - missing user credentials. Using PostgreSQL fallback.");
                        connectionString = "Host=localhost;Database=inventorymanagement;Username=postgres;Password=;Port=5432;SSL Mode=Prefer;Trust Server Certificate=true;";
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: Invalid DATABASE_URL format - missing host or credentials. Using PostgreSQL fallback.");
                    connectionString = "Host=localhost;Database=inventorymanagement;Username=postgres;Password=;Port=5432;SSL Mode=Prefer;Trust Server Certificate=true;";
                }
            }
            else
            {
                // Maybe it's a different format - try to use it directly as a connection string
                Console.WriteLine("DATABASE_URL doesn't start with postgresql://, trying as direct connection string...");
                
                // Check if it contains PostgreSQL connection string components
                if (trimmedUrl.Contains("Host=") || trimmedUrl.Contains("Server="))
                {
                    // Convert Server= to Host= if needed for PostgreSQL compatibility
                    connectionString = trimmedUrl.Replace("Server=", "Host=").Replace("User=", "Username=");
                    
                    // Ensure SSL mode is set for production
                    if (!connectionString.Contains("SSL Mode"))
                    {
                        connectionString += ";SSL Mode=Require;Trust Server Certificate=true;";
                    }
                    
                    Console.WriteLine("Using DATABASE_URL as direct connection string (converted for PostgreSQL)");
                }
                else
                {
                    Console.WriteLine("DATABASE_URL format not recognized. Using PostgreSQL fallback.");
                    connectionString = "Host=localhost;Database=inventorymanagement;Username=postgres;Password=;Port=5432;SSL Mode=Prefer;Trust Server Certificate=true;";
                }
            }
        }
        catch (UriFormatException ex)
        {
            Console.WriteLine($"Error parsing DATABASE_URL: {ex.Message}. Using PostgreSQL fallback.");
            connectionString = "Host=localhost;Database=inventorymanagement;Username=postgres;Password=;Port=5432;SSL Mode=Prefer;Trust Server Certificate=true;";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error with DATABASE_URL: {ex.Message}. Using PostgreSQL fallback.");
            connectionString = "Host=localhost;Database=inventorymanagement;Username=postgres;Password=;Port=5432;SSL Mode=Prefer;Trust Server Certificate=true;";
        }
    }
    else if (!string.IsNullOrWhiteSpace(mysqlUrl))
    {
        try
        {
            Console.WriteLine("Trying POSTGRES_URL or alternative URL...");
            var uri = new Uri(mysqlUrl.Trim());
            if (uri.Host != null && uri.UserInfo != null && uri.UserInfo.Contains(':'))
            {
                var userParts = uri.UserInfo.Split(':');
                if (userParts.Length == 2)
                {
                    var altPort = uri.Port > 0 ? uri.Port : 5432; // Default PostgreSQL port
                    connectionString = $"Host={uri.Host};Port={altPort};Database={uri.AbsolutePath.TrimStart('/')};Username={userParts[0]};Password={userParts[1]};SSL Mode=Require;Trust Server Certificate=true;";
                    Console.WriteLine("Successfully parsed alternative PostgreSQL URL");
                }
            }
            else
            {
                Console.WriteLine("Invalid alternative URL format. Using PostgreSQL fallback.");
                connectionString = "Host=localhost;Database=inventorymanagement;Username=postgres;Password=;Port=5432;SSL Mode=Prefer;Trust Server Certificate=true;";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing alternative PostgreSQL URL: {ex.Message}. Using PostgreSQL fallback.");
            connectionString = "Host=localhost;Database=inventorymanagement;Username=postgres;Password=;Port=5432;SSL Mode=Prefer;Trust Server Certificate=true;";
        }
    }
    else if (!string.IsNullOrWhiteSpace(dbHost) && !string.IsNullOrWhiteSpace(dbName) && !string.IsNullOrWhiteSpace(dbUser))
    {
        Console.WriteLine("Using individual DB environment variables for PostgreSQL...");
        connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword ?? ""};SSL Mode=Require;Trust Server Certificate=true;";
        Console.WriteLine("Successfully built PostgreSQL connection string from individual variables");
    }
    else if (!string.IsNullOrWhiteSpace(pgHost) && !string.IsNullOrWhiteSpace(pgDatabase) && !string.IsNullOrWhiteSpace(pgUser))
    {
        Console.WriteLine("Using PostgreSQL-specific environment variables...");
        var pgPortValue = !string.IsNullOrEmpty(pgPort) ? pgPort : "5432";
        connectionString = $"Host={pgHost};Port={pgPortValue};Database={pgDatabase};Username={pgUser};Password={pgPassword ?? ""};SSL Mode=Require;Trust Server Certificate=true;";
        Console.WriteLine("Successfully built PostgreSQL connection string from PG variables");
    }
    else
    {
        Console.WriteLine("Warning: No database environment variables found. Using default PostgreSQL connection string.");
        // Set a PostgreSQL default connection string
        connectionString = "Host=localhost;Database=inventorymanagement;Username=postgres;Password=;Port=5432;SSL Mode=Prefer;Trust Server Certificate=true;";
    }
    
    Console.WriteLine("Final PostgreSQL connection string (masked): " + (connectionString?.Replace(";Password=", ";Password=***") ?? "null"));
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsProduction())
    {
        options.UseNpgsql(connectionString, 
            npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null));
    }
    else
    {
        options.UseNpgsql(connectionString,
            npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null));
    }
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    options.User.RequireUniqueEmail = true;
    
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Add a simple health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Add a simple test endpoint
app.MapGet("/test", () => "Application is running!");

// Add root redirect to login if not authenticated
app.MapGet("/", (HttpContext context) => 
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        return Results.Redirect("/Inventory");
    }
    return Results.Redirect("/Account/Login");
});

app.MapControllerRoute(
    name: "itemsByInventory",
    pattern: "Item/Inventory/{inventoryId:int}",
    defaults: new { controller = "Item", action = "ByInventory" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inventory}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        // In production, add retry logic for database operations
        if (builder.Environment.IsProduction())
        {
            var retryCount = 0;
            const int maxRetries = 3;
            
            while (retryCount < maxRetries)
            {
                try
                {
                    await context.Database.MigrateAsync();
                    await SeedData.Initialize(services, builder.Configuration);
                    logger.LogInformation("Database initialized successfully");
                    break;
                }
                catch (Exception ex) when (retryCount < maxRetries - 1)
                {
                    retryCount++;
                    logger.LogWarning($"Database initialization attempt {retryCount} failed: {ex.Message}. Retrying in 5 seconds...");
                    await Task.Delay(5000);
                }
            }
        }
        else
        {
            try
            {
                await context.Database.MigrateAsync();
                await SeedData.Initialize(services, builder.Configuration);
                logger.LogInformation("Database initialized successfully");
            }
            catch (Npgsql.NpgsqlException npgsqlEx)
            {
                logger.LogWarning(npgsqlEx, "PostgreSQL connection failed in development. Skipping database initialization. Make sure PostgreSQL is running with correct credentials.");
                logger.LogInformation("Application will continue without database - some features may not work.");
            }
        }
    }
    catch (Npgsql.NpgsqlException npgsqlEx)
    {
        logger.LogError(npgsqlEx, "PostgreSQL connection error occurred while initializing the database.");
        if (builder.Environment.IsProduction())
        {
            logger.LogCritical("Application will continue without database initialization in production.");
        }
        else
        {
            throw;
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An unexpected error occurred while initializing the database.");
        if (builder.Environment.IsProduction())
        {
            logger.LogCritical("Application will continue without database initialization in production.");
        }
        else
        {
            throw;
        }
    }
}

// Configure port for Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
Console.WriteLine($"Starting application on port: {port}");
Console.WriteLine($"ASPNETCORE_URLS: {Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}");
Console.WriteLine($"DATABASE_URL exists: {!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DATABASE_URL"))}");
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();