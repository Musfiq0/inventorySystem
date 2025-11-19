using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Identity;
namespace InventoryManagement.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        private async Task<bool> IsCurrentUserAdmin()
        {
            if (User.Identity?.IsAuthenticated != true)
                return false;
            var user = await _userManager.GetUserAsync(User);
            return user?.IsAdmin ?? false;
        }
        public async Task<IActionResult> UserManagement()
        {
            if (!await IsCurrentUserAdmin())
                return Forbid();
            var users = await _context.Users.OrderBy(u => u.LastName ?? "").ThenBy(u => u.FirstName ?? "").ToListAsync();
            return View(users);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminStatus(string userId)
        {
            if (!await IsCurrentUserAdmin())
                return Forbid();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();
            var currentUser = await _userManager.GetUserAsync(User);
            if (user.Id == currentUser?.Id)
            {
                TempData["Error"] = "You cannot change your own admin status.";
                return RedirectToAction(nameof(UserManagement));
            }
            user.IsAdmin = !user.IsAdmin;
            await _userManager.UpdateAsync(user);
            if (user.IsAdmin)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            TempData["Success"] = $"{user.FullName ?? "User"} admin status updated.";
            return RedirectToAction(nameof(UserManagement));
        }

        public async Task<IActionResult> AllInventories()
        {
            if (!await IsCurrentUserAdmin())
                return Forbid();

            var inventories = await _context.Inventories
                .Include(i => i.Items)
                .OrderBy(i => i.Name)
                .ToListAsync();
            return View(inventories);
        }

        public async Task<IActionResult> AllItems()
        {
            if (!await IsCurrentUserAdmin())
                return Forbid();

            var items = await _context.Items
                .Include(i => i.Inventory)
                .OrderBy(i => i.Name)
                .ToListAsync();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            if (!await IsCurrentUserAdmin())
                return Forbid();

            var inventory = await _context.Inventories.Include(i => i.Items).FirstOrDefaultAsync(i => i.Id == id);
            if (inventory == null)
                return NotFound();

            _context.Items.RemoveRange(inventory.Items);
            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Inventory '{inventory.Name}' and all its items deleted successfully.";
            return RedirectToAction(nameof(AllInventories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            if (!await IsCurrentUserAdmin())
                return Forbid();

            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Item '{item.Name}' deleted successfully.";
            return RedirectToAction(nameof(AllItems));
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!await IsCurrentUserAdmin())
                return Forbid();

            var stats = new
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalInventories = await _context.Inventories.CountAsync(),
                TotalItems = await _context.Items.CountAsync(),
                AdminUsers = await _context.Users.CountAsync(u => u.IsAdmin),
                RecentUsers = await _context.Users.OrderByDescending(u => u.CreatedAt).Take(5).ToListAsync(),
                RecentInventories = await _context.Inventories.OrderByDescending(i => i.CreatedAt).Take(5).ToListAsync(),
                RecentItems = await _context.Items.Include(i => i.Inventory).OrderByDescending(i => i.CreatedAt).Take(5).ToListAsync()
            };

            return View(stats);
        }
    }
}
