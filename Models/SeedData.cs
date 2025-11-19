using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagement.Models
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var context = serviceProvider.GetRequiredService<AppDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            try
            {
                if (await context.Inventories.AnyAsync())
                {
                    return;
                }

                await CreateRoles(roleManager);
                
                await CreateAdminUser(userManager);
                
                CreateSiteContent(context);

                await CreateSampleData(context);
                await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "User" };

            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task CreateAdminUser(UserManager<ApplicationUser> userManager)
        {
            string adminEmail = "admin@inventory.com";
            string adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    CreatedAt = DateTime.UtcNow,
                    IsAdmin = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static void CreateSiteContent(AppDbContext context)
        {
            var siteContents = new List<SiteContent>
            {
                new SiteContent
                {
                    Key = "SiteTitle",
                    Value = "Inventory Management System",
                    Description = "Main site title displayed in header"
                },
                new SiteContent
                {
                    Key = "SiteDescription",
                    Value = "Organize, track, and manage your inventories with ease. Create categories, add items, and keep track of everything in one place.",
                    Description = "Site description shown on homepage"
                },
                new SiteContent
                {
                    Key = "FooterText",
                    Value = "Simple inventory management solution.",
                    Description = "Footer description text"
                },
                new SiteContent
                {
                    Key = "CompanyName",
                    Value = "Inventory Management",
                    Description = "Company name for branding"
                }
            };

            context.SiteContents.AddRange(siteContents);
        }

        private static async Task CreateSampleData(AppDbContext context)
        {
            var officeInventory = new Inventory
            {
                Name = "Office Supplies",
                Description = "General office supplies and equipment",
                CreatedAt = DateTime.UtcNow
            };

            var electronicsInventory = new Inventory
            {
                Name = "Electronics",
                Description = "Computer equipment and electronic devices",
                CreatedAt = DateTime.UtcNow
            };

            var warehouseInventory = new Inventory
            {
                Name = "Warehouse Storage",
                Description = "Large items and bulk storage",
                CreatedAt = DateTime.UtcNow
            };

            context.Inventories.AddRange(officeInventory, electronicsInventory, warehouseInventory);
            await context.SaveChangesAsync();

            var officeItems = new List<Item>
            {
                new Item
                {
                    InventoryId = officeInventory.Id,
                    Name = "Ballpoint Pens (Black)",
                    Description = "Pack of 12 black ballpoint pens",
                    Quantity = 50,
                    Price = 8.99m,
                    CustomId = Item.GenerateCustomId(0),
                    Category = "Office Supplies",
                    SKU = "PEN-001",
                    CreatedAt = DateTime.UtcNow
                },
                new Item
                {
                    InventoryId = officeInventory.Id,
                    Name = "A4 Copy Paper",
                    Description = "500 sheets of white A4 copy paper",
                    Quantity = 25,
                    Price = 12.99m,
                    CustomId = Item.GenerateCustomId(1),
                    Category = "Office Supplies",
                    SKU = "PAPER-001",
                    CreatedAt = DateTime.UtcNow
                },
                new Item
                {
                    InventoryId = officeInventory.Id,
                    Name = "Stapler",
                    Description = "Heavy-duty metal stapler",
                    Quantity = 8,
                    Price = 24.99m,
                    CustomId = Item.GenerateCustomId(2),
                    Category = "Office Supplies",
                    SKU = "STAPLER-001",
                    CreatedAt = DateTime.UtcNow
                },
                new Item
                {
                    InventoryId = officeInventory.Id,
                    Name = "Sticky Notes",
                    Description = "Yellow 3x3 inch sticky notes, pack of 12",
                    Quantity = 15,
                    Price = 6.49m,
                    CustomId = Item.GenerateCustomId(3),
                    Category = "Office Supplies",
                    SKU = "NOTES-001",
                    CreatedAt = DateTime.UtcNow
                }
            };

            var electronicsItems = new List<Item>
            {
                new Item
                {
                    InventoryId = electronicsInventory.Id,
                    Name = "Wireless Mouse",
                    Description = "Ergonomic wireless optical mouse",
                    Quantity = 12,
                    Price = 29.99m,
                    CustomId = "MOUSE001",
                    Category = "Computer Accessories",
                    SKU = "MOUSE-WL-001",
                    CreatedAt = DateTime.UtcNow
                },
                new Item
                {
                    InventoryId = electronicsInventory.Id,
                    Name = "USB-C Cable",
                    Description = "3-foot USB-C to USB-A cable",
                    Quantity = 20,
                    Price = 15.99m,
                    CustomId = "CABLE001",
                    Category = "Cables",
                    SKU = "CABLE-USBC-001",
                    CreatedAt = DateTime.UtcNow
                },
                new Item
                {
                    InventoryId = electronicsInventory.Id,
                    Name = "Bluetooth Headphones",
                    Description = "Noise-cancelling wireless headphones",
                    Quantity = 5,
                    Price = 149.99m,
                    CustomId = "AUDIO001",
                    Category = "Audio Equipment",
                    SKU = "HEADPHONE-BT-001",
                    CreatedAt = DateTime.UtcNow
                },
                new Item
                {
                    InventoryId = electronicsInventory.Id,
                    Name = "Laptop Stand",
                    Description = "Adjustable aluminum laptop stand",
                    Quantity = 3,
                    Price = 79.99m,
                    CustomId = "STAND001",
                    Category = "Computer Accessories",
                    SKU = "STAND-LP-001",
                    CreatedAt = DateTime.UtcNow
                }
            };

            var warehouseItems = new List<Item>
            {
                new Item
                {
                    InventoryId = warehouseInventory.Id,
                    Name = "Storage Boxes (Large)",
                    Description = "Heavy-duty cardboard storage boxes",
                    Quantity = 100,
                    Price = 4.99m,
                    CustomId = "BOX001",
                    Category = "Storage",
                    SKU = "BOX-LG-001",
                    CreatedAt = DateTime.UtcNow
                },
                new Item
                {
                    InventoryId = warehouseInventory.Id,
                    Name = "Bubble Wrap",
                    Description = "500ft roll of bubble wrap",
                    Quantity = 15,
                    Price = 39.99m,
                    CustomId = "WRAP001",
                    Category = "Packaging",
                    SKU = "WRAP-BB-001",
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Items.AddRange(officeItems);
            context.Items.AddRange(electronicsItems);
            context.Items.AddRange(warehouseItems);
        }
    }
}