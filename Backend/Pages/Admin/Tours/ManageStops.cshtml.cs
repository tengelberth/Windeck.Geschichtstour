using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Pages.Admin.Tours
{
    /// <summary>
    /// Verwaltung der Stops (Stationszuordnung) einer Tour.
    /// Hier können Stationen zur Tour hinzugefügt, entfernt und
    /// ihre Reihenfolge festgelegt werden.
    /// </summary>
    public class ManageStopsModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        public ManageStopsModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Die Tour, deren Stops verwaltet werden.
        /// </summary>
        public Tour Tour { get; set; } = default!;

        /// <summary>
        /// Alle Stops der Tour, sortiert nach der Reihenfolge.
        /// </summary>
        public List<TourStop> Stops { get; set; } = new();

        /// <summary>
        /// Dropdown-Liste mit allen Stationen, die als Stop hinzugefügt werden können.
        /// </summary>
        public List<SelectListItem> StationOptions { get; set; } = new();

        // Diese Properties werden für das "Add Stop"-Formular gebunden:

        /// <summary>
        /// ID der Station, die als neuer Stop hinzugefügt werden soll.
        /// </summary>
        [BindProperty]
        public int SelectedStationId { get; set; }

        /// <summary>
        /// Reihenfolge, an der der neue Stop in der Tour eingefügt werden soll.
        /// </summary>
        [BindProperty]
        public int? NewStopOrder { get; set; }

        /// <summary>
        /// Lädt Tour, Stops und die verfügbaren Stationen.
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Tour inkl. Stops und Stations laden
            Tour = await _dbContext.Tours
                .Include(t => t.Stops)
                    .ThenInclude(ts => ts.Station)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (Tour == null)
            {
                return NotFound();
            }

            Stops = Tour.Stops
                .OrderBy(ts => ts.Order)
                .ToList();

            await LoadStationOptionsAsync();

            return Page();
        }

        /// <summary>
        /// Fügt der Tour einen neuen Stop hinzu.
        /// </summary>
        public async Task<IActionResult> OnPostAddStopAsync(int id)
        {
            // Tour laden
            Tour = await _dbContext.Tours
                .Include(t => t.Stops)
                .ThenInclude(ts => ts.Station)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (Tour == null)
            {
                return NotFound();
            }

            await LoadStationOptionsAsync();

            if (!ModelState.IsValid)
            {
                // Falls schon Validierungsfehler vorliegen, Seite erneut anzeigen
                Stops = Tour.Stops.OrderBy(s => s.Order).ToList();
                return Page();
            }

            // Station prüfen
            var station = await _dbContext.Stations.FindAsync(SelectedStationId);
            if (station == null)
            {
                ModelState.AddModelError(string.Empty, "Die ausgewählte Station existiert nicht.");
                Stops = Tour.Stops.OrderBy(s => s.Order).ToList();
                return Page();
            }

            // Falls keine Order angegeben: ans Ende setzen
            int order;
            if (NewStopOrder.HasValue && NewStopOrder.Value > 0)
            {
                order = NewStopOrder.Value;
            }
            else
            {
                order = (Tour.Stops.Any() ? Tour.Stops.Max(s => s.Order) : 0) + 1;
            }

            // Prüfen, ob die Order bereits verwendet wird
            if (Tour.Stops.Any(s => s.Order == order))
            {
                ModelState.AddModelError(string.Empty,
                    $"Die Reihenfolge {order} wird bereits von einem anderen Stop verwendet. Bitte eine andere Zahl wählen.");

                Stops = Tour.Stops.OrderBy(s => s.Order).ToList();
                return Page();
            }

            var stop = new TourStop
            {
                TourId = Tour.Id,
                StationId = station.Id,
                Order = order
            };

            _dbContext.TourStops.Add(stop);
            TempData["SuccessMessage"] = "Tour-Stop wurde erfolgreich aktualisiert.";
            await _dbContext.SaveChangesAsync();

            return RedirectToPage(new { id });
        }


        /// <summary>
        /// Löscht einen Stop aus der Tour.
        /// </summary>
        public async Task<IActionResult> OnPostDeleteStopAsync(int id, int stopId)
        {
            var stop = await _dbContext.TourStops
                .FirstOrDefaultAsync(ts => ts.Id == stopId && ts.TourId == id);

            if (stop == null)
            {
                return NotFound();
            }

            _dbContext.TourStops.Remove(stop);
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tour-Stop wurde gelöscht.";

            return RedirectToPage(new { id });
        }

        /// <summary>
        /// Aktualisiert die Reihenfolge eines bestehenden Stops.
        /// (Einfache Variante: ein Stop pro Request)
        /// </summary>
        public async Task<IActionResult> OnPostUpdateOrderAsync(int id, int stopId, int newOrder)
        {
            // Tour + Stops laden
            Tour = await _dbContext.Tours
                .Include(t => t.Stops)
                .ThenInclude(ts => ts.Station)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (Tour == null)
            {
                return NotFound();
            }

            await LoadStationOptionsAsync();

            var stop = Tour.Stops.FirstOrDefault(ts => ts.Id == stopId);
            if (stop == null)
            {
                return NotFound();
            }

            if (newOrder <= 0)
            {
                ModelState.AddModelError(string.Empty,
                    "Die Reihenfolge muss eine positive Zahl größer oder gleich 1 sein.");
                Stops = Tour.Stops.OrderBy(s => s.Order).ToList();
                return Page();
            }

            // Prüfen, ob ein anderer Stop bereits diese Order hat
            if (Tour.Stops.Any(s => s.Id != stopId && s.Order == newOrder))
            {
                ModelState.AddModelError(string.Empty,
                    $"Die Reihenfolge {newOrder} wird bereits von einem anderen Stop verwendet. Bitte eine andere Zahl wählen.");
                Stops = Tour.Stops.OrderBy(s => s.Order).ToList();
                return Page();
            }

            stop.Order = newOrder;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tour-Stop wurde erfolgreich aktualisiert.";

            return RedirectToPage(new { id });
        }


        /// <summary>
        /// Lädt alle Stationen aus der Datenbank für das Dropdown.
        /// </summary>
        private async Task LoadStationOptionsAsync()
        {
            StationOptions = await _dbContext.Stations
                .OrderBy(s => s.Title)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Title
                })
                .ToListAsync();
        }
    }
}
