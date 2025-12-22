using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Pages.Admin.Tours
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        public IndexModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IList<Tour> Tours { get; set; } = new List<Tour>();

        public async Task OnGetAsync()
        {
            Tours = await _dbContext.Tours
                .Include(t => t.Stops)
                .OrderBy(t => t.Title)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var tour = await _dbContext.Tours
                .Include(t => t.Stops)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null)
                return NotFound();

            if (tour.Stops.Any())
            {
                _dbContext.TourStops.RemoveRange(tour.Stops);
            }

            _dbContext.Tours.Remove(tour);
            TempData["SuccessMessage"] = "Tour wurde gelöscht.";
            await _dbContext.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
