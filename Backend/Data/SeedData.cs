using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Data
{
    /// <summary>
    /// Enthält Logik zum Initialbefüllen der Datenbank mit Startinhalten
    /// (Stationen, Kategorien, Medien, Touren).
    /// Wird beim Start der Anwendung einmalig ausgeführt.
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Führt das Seed-Skript aus, falls noch keine Stationen vorhanden sind.
        /// </summary>
        public static void Initialize(AppDbContext context)
        {
            // Falls bereits Stationen existieren, gehen wir davon aus,
            // dass die Datenbank schon befüllt ist und machen nichts.
            if (context.Stations.Any())
            {
                return;
            }

            // ----------------------------
            // Kategorien anlegen
            // ----------------------------
            var ortsGeschichte = new Category
            {
                Name = "Ortsgeschichte",
                Description = "Historische Gebäude und Orte im Ortskern."
            };

            var industrieGeschichte = new Category
            {
                Name = "Industriegeschichte",
                Description = "Industrie- und Verkehrsgeschichte entlang der Sieg."
            };

            context.Categories.AddRange(ortsGeschichte, industrieGeschichte);
            context.SaveChanges();

            // ----------------------------
            // Stationen anlegen
            // (Koordinaten sind initiale Referenzwerte und können später präzisiert werden)
            // ----------------------------

            var rathausRosbach = new Station
            {
                Code = "RATHAUS_ROSBACH",
                Title = "Rathaus Rosbach",
                ShortDescription = "Zentrales Verwaltungsgebäude der Gemeinde Windeck.",
                LongDescription = "Das Rathaus in Rosbach ist Sitz der Gemeindeverwaltung Windeck. "
                    + "Hier werden seit vielen Jahren die Geschicke der Gemeinde gelenkt. "
                    + "Das Gebäude steht sinnbildlich für die Entwicklung der kommunalen Selbstverwaltung.",
                Street = "Hauptstraße",
                HouseNumber = "1",
                ZipCode = "51570",
                City = "Windeck-Rosbach",
                Latitude = 50.8,
                Longitude = 7.58,
                Category = ortsGeschichte
            };

            var marktplatzRosbach = new Station
            {
                Code = "MARKTPLATZ_ROSBACH",
                Title = "Marktplatz Rosbach",
                ShortDescription = "Historischer Treffpunkt im Ortskern von Rosbach.",
                LongDescription = "Der Marktplatz war über viele Jahrzehnte ein zentraler Ort "
                    + "für Handel, Begegnung und Veranstaltungen. Märkte, Feste und politische "
                    + "Veranstaltungen fanden hier statt und prägten das gesellschaftliche Leben.",
                City = "Windeck-Rosbach",
                Latitude = 50.8005,
                Longitude = 7.581,
                Category = ortsGeschichte
            };

            var burgWindeck = new Station
            {
                Code = "BURG_WINDECK",
                Title = "Burg Windeck",
                ShortDescription = "Historische Burgruine mit Blick über das Siegtal.",
                LongDescription = "Die Burg Windeck diente über Jahrhunderte als strategischer "
                    + "Punkt zur Kontrolle des Siegtals. Heute ist sie ein beliebtes Ausflugsziel "
                    + "und bietet einen eindrucksvollen Blick über die Region.",
                City = "Windeck",
                Latitude = 50.79,
                Longitude = 7.56,
                Category = ortsGeschichte
            };

            var alterBahnhofSchladern = new Station
            {
                Code = "ALTER_BAHNHOF_SCHLADERN",
                Title = "Alter Bahnhof Schladern",
                ShortDescription = "Ehemaliger Bahnhof, heute Kultur- und Veranstaltungsort.",
                LongDescription = "Der alte Bahnhof in Schladern war lange Zeit ein wichtiger Verkehrsknoten "
                    + "für die Region. Heute wird das Gebäude kulturell genutzt und zeigt, wie historische "
                    + "Infrastruktur neu belebt werden kann.",
                City = "Windeck-Schladern",
                Latitude = 50.79,
                Longitude = 7.57,
                Category = industrieGeschichte
            };

            context.Stations.AddRange(rathausRosbach, marktplatzRosbach, burgWindeck, alterBahnhofSchladern);
            context.SaveChanges();

            // ----------------------------
            // Medien anlegen
            // Die URLs zeigen auf erwartete Zielpfade und sollten bei Bedarf
            // durch reale Dateien im Deployment ersetzt werden.
            // ----------------------------

            var mediaItems = new[]
            {
                new MediaItem
                {
                    Station = rathausRosbach,
                    MediaType = "Image",
                    Url = "/media/images/rathaus_rosbach_1.jpg",
                    Caption = "Ansicht des Rathauses Rosbach",
                    SortOrder = 1
                },
                new MediaItem
                {
                    Station = burgWindeck,
                    MediaType = "Image",
                    Url = "/media/images/burg_windeck_1.jpg",
                    Caption = "Blick auf die Burgruine Windeck",
                    SortOrder = 1
                },
                new MediaItem
                {
                    Station = alterBahnhofSchladern,
                    MediaType = "Image",
                    Url = "/media/images/alter_bahnhof_schladern_1.jpg",
                    Caption = "Alter Bahnhof Schladern",
                    SortOrder = 1
                }
            };

            context.MediaItems.AddRange(mediaItems);
            context.SaveChanges();

            // ----------------------------
            // Touren anlegen
            // ----------------------------

            var altstadtTourRosbach = new Tour
            {
                Title = "Altstadttour Rosbach",
                Description = "Kurzer Rundgang durch den historischen Ortskern von Rosbach."
            };

            var industrieTourSieg = new Tour
            {
                Title = "Industrie an der Sieg",
                Description = "Tour zu Standorten der Industrie- und Verkehrsgeschichte entlang der Sieg."
            };

            context.Tours.AddRange(altstadtTourRosbach, industrieTourSieg);
            context.SaveChanges();

            // ----------------------------
            // TourStops anlegen (Reihenfolge innerhalb der Tour)
            // ----------------------------

            var tourStops = new[]
            {
                // Altstadttour Rosbach
                new TourStop
                {
                    Tour = altstadtTourRosbach,
                    Station = rathausRosbach,
                    Order = 1
                },
                new TourStop
                {
                    Tour = altstadtTourRosbach,
                    Station = marktplatzRosbach,
                    Order = 2
                },
                new TourStop
                {
                    Tour = altstadtTourRosbach,
                    Station = burgWindeck,
                    Order = 3
                },

                // Industrie-Tour
                new TourStop
                {
                    Tour = industrieTourSieg,
                    Station = alterBahnhofSchladern,
                    Order = 1
                },
                new TourStop
                {
                    Tour = industrieTourSieg,
                    Station = burgWindeck, // Als Abschluss mit Blick ins Tal
                    Order = 2
                }
            };

            context.TourStops.AddRange(tourStops);
            context.SaveChanges();
        }
    }
}

