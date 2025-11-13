using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Models
{
    // Creates sample inventory data for demonstration and testing
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var context = serviceProvider.GetRequiredService<AppDbContext>();

            try
            {
                // Check if we already have data
                if (await context.Inventories.AnyAsync())
                {
                    return; // Database already has data
                }

                await CreateSampleData(context);
                await context.SaveChangesAsync();

                Console.WriteLine("Database seeded successfully with sample inventories and items.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
                throw; // Re-throw to handle in Program.cs
            }
        }

        /// Create sample inventory and items for testing and demonstration
        /// <param name="context">Database context</param>
        private static async Task CreateSampleData(AppDbContext context)
        {
            // Create sample inventories
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

            // Add inventories to database
            context.Inventories.AddRange(officeInventory, electronicsInventory, warehouseInventory);
            await context.SaveChangesAsync(); // Save to get inventory IDs

            // Create sample items for Office Supplies inventory
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

            // Create sample items for Electronics inventory
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

            // Create sample items for Warehouse inventory
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

            // Add all items to database
            context.Items.AddRange(officeItems);
            context.Items.AddRange(electronicsItems);
            context.Items.AddRange(warehouseItems);

            Console.WriteLine("Sample data created:");
            Console.WriteLine($"- {officeInventory.Name}: {officeItems.Count} items");
            Console.WriteLine($"- {electronicsInventory.Name}: {electronicsItems.Count} items");
            Console.WriteLine($"- {warehouseInventory.Name}: {warehouseItems.Count} items");
        }
    }
}