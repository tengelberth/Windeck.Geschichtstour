using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Data
{
    /// <summary>
    /// Zentrale Entity-Framework-Core-DbContext-Klasse.
    /// Kapselt den Zugriff auf die relationale Datenbank.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Konstruktor, der die DbContextOptions (z. B. Connection String)
        /// über Dependency Injection erhält.
        /// </summary>
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Tabelle für Stationen (historische Orte).
        /// </summary>
        public DbSet<Station> Stations => Set<Station>();

        /// <summary>
        /// Tabelle für Kategorien.
        /// </summary>
        public DbSet<Category> Categories => Set<Category>();

        /// <summary>
        /// Tabelle für Medienobjekte (Bilder, Audio, Video).
        /// </summary>
        public DbSet<MediaItem> MediaItems => Set<MediaItem>();

        /// <summary>
        /// Tabelle für Touren.
        /// </summary>
        public DbSet<Tour> Tours => Set<Tour>();

        /// <summary>
        /// Tabelle für TourStops (Verknüpfung Tour ↔ Station).
        /// </summary>
        public DbSet<TourStop> TourStops => Set<TourStop>();

        /// <summary>
        /// Feinere Konfiguration des Datenmodells, z. B. Relationen und Indizes.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Station.Code soll eindeutig sein, damit jeder QR-Code eindeutig
            // auf eine Station zeigt.
            modelBuilder.Entity<Station>()
                .HasIndex(s => s.Code)
                .IsUnique();

            // Beziehung: Tour 1:n TourStops
            modelBuilder.Entity<TourStop>()
                .HasOne(ts => ts.Tour)
                .WithMany(t => t.Stops)
                .HasForeignKey(ts => ts.TourId)
                .OnDelete(DeleteBehavior.Cascade);

            // Beziehung: Station 1:n TourStops
            // Eine Station kann in mehreren Touren vorkommen.
            modelBuilder.Entity<TourStop>()
                .HasOne(ts => ts.Station)
                .WithMany(s => s.TourStops)
                .HasForeignKey(ts => ts.StationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
