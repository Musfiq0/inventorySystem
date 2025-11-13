using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers
{
    public class ItemController : Controller
    {
        private readonly AppDbContext _context;

        public ItemController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Item (shows items from all inventories)
        public async Task<IActionResult> Index(string search, string category, string stock, string sort)
        {
            var items = _context.Items.Include(i => i.Inventory).AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                items = items.Where(i => i.Name.Contains(search) || 
                                       i.Description.Contains(search) || 
                                       i.SKU.Contains(search) ||
                                       i.Inventory.Name.Contains(search));
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(category))
            {
                items = items.Where(i => i.Category == category);
            }

            // Apply stock filter
            if (!string.IsNullOrEmpty(stock))
            {
                items = stock switch
                {
                    "low" => items.Where(i => i.Quantity < 10 && i.Quantity > 0),
                    "out" => items.Where(i => i.Quantity == 0),
                    "good" => items.Where(i => i.Quantity >= 10),
                    _ => items
                };
            }

            // Apply sorting
            items = sort switch
            {
                "quantity" => items.OrderBy(i => i.Quantity),
                "price" => items.OrderBy(i => i.Price),
                "date" => items.OrderByDescending(i => i.CreatedAt),
                _ => items.OrderBy(i => i.Name)
            };

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentStock = stock;
            ViewBag.CurrentSort = sort;
            ViewBag.TotalItems = await items.CountAsync();

            return View(await items.ToListAsync());
        }

        // GET: Item/ByInventory/{inventoryId} (shows items for specific inventory)
        public async Task<IActionResult> ByInventory(int inventoryId, string search, string category, string stock, string sort)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null)
            {
                return NotFound();
            }

            var items = _context.Items.Where(i => i.InventoryId == inventoryId).AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                items = items.Where(i => i.Name.Contains(search) || 
                                       i.Description.Contains(search) || 
                                       i.SKU.Contains(search));
            }

            // Apply category filter  
            if (!string.IsNullOrEmpty(category))
            {
                items = items.Where(i => i.Category == category);
            }

            // Apply stock filter
            if (!string.IsNullOrEmpty(stock))
            {
                items = stock switch
                {
                    "low" => items.Where(i => i.Quantity < 10 && i.Quantity > 0),
                    "out" => items.Where(i => i.Quantity == 0),
                    "good" => items.Where(i => i.Quantity >= 10),
                    _ => items
                };
            }

            // Apply sorting
            items = sort switch
            {
                "quantity" => items.OrderBy(i => i.Quantity),
                "price" => items.OrderBy(i => i.Price),
                "date" => items.OrderByDescending(i => i.CreatedAt),
                _ => items.OrderBy(i => i.Name)
            };

            ViewBag.Inventory = inventory;
            ViewBag.InventoryId = inventoryId;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentStock = stock;
            ViewBag.CurrentSort = sort;
            ViewBag.TotalItems = await items.CountAsync();
            ViewBag.CanEdit = true; // Set edit permissions

            return View("Index", await items.ToListAsync());
        }

        public IActionResult Create(int inventoryId)
        {
            ViewBag.InventoryId = inventoryId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int inventoryId, Item item)
        {
            item.InventoryId = inventoryId;
            var existingItems = await _context.Items.Where(i => i.InventoryId == inventoryId).ToListAsync();
            item.CustomId = $"ITEM{(existingItems.Count + 1):D3}";

            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Inventory", new { id = inventoryId });
            }

            ViewBag.InventoryId = inventoryId;
            return View(item);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item item)
        {
            if (id != item.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Items.Any(e => e.Id == item.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction("Details", "Inventory", new { id = item.InventoryId });
            }
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                var inventoryId = item.InventoryId;
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Item '{item.Name}' has been deleted successfully.";
                return RedirectToAction("Details", "Inventory", new { id = inventoryId });
            }
            TempData["Error"] = "Item not found.";
            return RedirectToAction("Index", "Inventory");
        }
    }
}
