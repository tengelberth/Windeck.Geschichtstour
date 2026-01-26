using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Pages.Admin.Tours
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        public EditModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty]
        public Tour Tour { get; set; } = default!;

        public bool IsNew => Tour.Id == 0;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                Tour = new Tour();
            }
            else
            {
                Tour = await _dbContext.Tours
                    .Include(t => t.Stops)
                    .FirstOrDefaultAsync(t => t.Id == id.Value);

                if (Tour == null)
                    return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Tour.Id == 0)
            {
                _dbContext.Tours.Add(Tour);
                TempData["SuccessMessage"] = "Tour wurde erfolgreich angelegt.";
            }
            else
            {
                _dbContext.Attach(Tour).State = EntityState.Modified;
                TempData["SuccessMessage"] = "Tour wurde erfolgreich aktualisiert.";
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
