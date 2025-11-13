using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers
{
    public class InventoryController : Controller
    {
        private readonly AppDbContext _context;

        public InventoryController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search)
        {
            var inventories = await _context.Inventories.Include(i => i.Items).ToListAsync();
            
            if (!string.IsNullOrEmpty(search))
            {
                inventories = inventories.Where(i => 
                    i.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(i.Description) && i.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                
                ViewBag.CurrentSearch = search;
            }
            
            return View(inventories);
        }

        public async Task<IActionResult> Details(int id)
        {
            var inventory = await _context.Inventories.Include(i => i.Items).FirstOrDefaultAsync(i => i.Id == id);
            if (inventory == null) return NotFound();

            var model = new InventoryDetailsViewModel
            {
                Inventory = inventory,
                Items = inventory.Items?.ToList() ?? new List<Item>()
            };
            return View(model);
        }

        public IActionResult Create()
        {
            return View(new Inventory());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                inventory.CreatedAt = DateTime.Now;
                _context.Add(inventory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inventory);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null) return NotFound();
            return View(inventory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inventory inventory)
        {
            if (id != inventory.Id) return NotFound();
            
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inventory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Inventories.Any(e => e.Id == inventory.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(inventory);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var inventory = await _context.Inventories.Include(i => i.Items).FirstOrDefaultAsync(i => i.Id == id);
            if (inventory == null) return NotFound();
            return View(inventory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory != null)
            {
                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
