using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Pages.Admin.Categories
{
    /// <summary>
    /// Übersicht und Verwaltung der Kategorien.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        public IndexModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IList<Category> Categories { get; set; } = new List<Category>();

        public async Task OnGetAsync()
        {
            Categories = await _dbContext.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var category = await _dbContext.Categories
                .Include(c => c.Stations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            // Stationen sollen nicht mit gelöscht werden, daher Kategorie-Referenz entfernen
            foreach (var station in category.Stations)
            {
                station.CategoryId = null;
            }

            _dbContext.Categories.Remove(category);
            TempData["SuccessMessage"] = "Kategorie wurde gelöscht.";
            await _dbContext.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
