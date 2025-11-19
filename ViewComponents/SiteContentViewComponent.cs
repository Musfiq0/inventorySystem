using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Models;

namespace InventoryManagement.ViewComponents
{
    public class SiteContentViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public SiteContentViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string key)
        {
            var content = await _context.SiteContents.FirstOrDefaultAsync(s => s.Key == key);
            var value = content?.Value ?? key; // Fallback to key if not found
            
            return View("Default", new { Key = key, Value = value });
        }
    }
}