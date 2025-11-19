using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagement.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(AppDbContext context, UserManager<ApplicationUser> userManager, ILogger<InventoryController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }
        
        [AllowAnonymous]
        [Route("debug/env")]
        public IActionResult DebugEnvironment()
        {
            if (!HttpContext.Request.Host.Host.Contains("render.com"))
            {
                return NotFound(); // Only allow this on Render
            }
            
            var envVars = Environment.GetEnvironmentVariables();
            var dbRelatedVars = new Dictionary<string, string>();
            
            foreach (var key in envVars.Keys)
            {
                var keyStr = key.ToString() ?? "";
                if (keyStr.Contains("DATABASE", StringComparison.OrdinalIgnoreCase) ||
                    keyStr.Contains("MYSQL", StringComparison.OrdinalIgnoreCase) ||
                    keyStr.Contains("DB_", StringComparison.OrdinalIgnoreCase) ||
                    keyStr.Contains("ASPNET", StringComparison.OrdinalIgnoreCase))
                {
                    var value = envVars[key]?.ToString() ?? "";
                    // Mask sensitive information
                    if (keyStr.Contains("PASSWORD", StringComparison.OrdinalIgnoreCase) || 
                        keyStr.Contains("URL", StringComparison.OrdinalIgnoreCase))
                    {
                        value = value.Length > 0 ? "***MASKED***" : "EMPTY";
                    }
                    dbRelatedVars[keyStr] = value;
                }
            }
            
            return Json(new { 
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                DatabaseEnvironmentVariables = dbRelatedVars,
                Host = HttpContext.Request.Host.ToString()
            });
        }
        
        [AllowAnonymous]
        [Route("health")]
        public IActionResult Health()
        {
            return Json(new { 
                status = "healthy",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }
        
        [AllowAnonymous]
        public async Task<IActionResult> Index(string search)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error in Inventory Index");
                ViewBag.DatabaseError = "Database temporarily unavailable";
                return View(new List<Inventory>());
            }
        }
        [AllowAnonymous]
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
                var user = await _userManager.GetUserAsync(User);
                inventory.CreatedAt = DateTime.Now;
                inventory.CreatedBy = user?.Id;
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
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (inventory.CreatedBy != currentUser?.Id && !currentUser?.IsAdmin == true)
            {
                return Forbid();
            }
            
            return View(inventory);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inventory inventory)
        {
            if (id != inventory.Id) return NotFound();
            
            var existingInventory = await _context.Inventories.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
            if (existingInventory == null) return NotFound();
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (existingInventory.CreatedBy != currentUser?.Id && !currentUser?.IsAdmin == true)
            {
                return Forbid();
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    inventory.CreatedBy = existingInventory.CreatedBy;
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
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (inventory.CreatedBy != currentUser?.Id && !currentUser?.IsAdmin == true)
            {
                return Forbid();
            }
            
            return View(inventory);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory != null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (inventory.CreatedBy != currentUser?.Id && !currentUser?.IsAdmin == true)
                {
                    return Forbid();
                }
                
                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
