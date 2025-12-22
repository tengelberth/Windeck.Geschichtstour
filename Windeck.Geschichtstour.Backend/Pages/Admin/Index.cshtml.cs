using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;

namespace Windeck.Geschichtstour.Backend.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        public IndexModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int StationCount { get; set; }
        public int TourCount { get; set; }
        public int CategoryCount { get; set; }
        public int MediaCount { get; set; }

        public async Task OnGetAsync()
        {
            StationCount = await _dbContext.Stations.CountAsync();
            TourCount = await _dbContext.Tours.CountAsync();
            CategoryCount = await _dbContext.Categories.CountAsync();
            MediaCount = await _dbContext.MediaItems.CountAsync();
        }
    }
}
