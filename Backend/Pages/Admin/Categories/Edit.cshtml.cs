using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Pages.Admin.Categories
{
    /// <summary>
    /// Anlegen und Bearbeiten einer Kategorie.
    /// </summary>
    public class EditModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        public EditModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty]
        public Category Category { get; set; } = default!;

        public bool IsNew => Category.Id == 0;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                Category = new Category();
            }
            else
            {
                Category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id.Value);
                if (Category == null)
                    return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Category.Id == 0)
            {
                _dbContext.Categories.Add(Category);
                TempData["SuccessMessage"] = "Kategorie wurde erfolgreich angelegt.";
            }
            else
            {
                _dbContext.Attach(Category).State = EntityState.Modified;
                TempData["SuccessMessage"] = "Kategorie wurde erfolgreich aktualisiert.";
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
