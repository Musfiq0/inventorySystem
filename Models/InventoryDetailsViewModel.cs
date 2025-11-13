namespace InventoryManagement.Models;

// View model for inventory details page
// Contains inventory data and its associated items
public class InventoryDetailsViewModel
{
    public Inventory Inventory { get; set; } = null!;
    public List<Item> Items { get; set; } = new();
    public string? SearchTerm { get; set; }
}