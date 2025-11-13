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
