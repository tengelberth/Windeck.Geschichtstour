namespace Windeck.Geschichtstour.Backend.Models
{
    /// <summary>
    /// Verbindet eine Tour mit einer Station und legt die Reihenfolge fest,
    /// in der die Station innerhalb der Tour besucht werden soll.
    /// </summary>
    public class TourStop
    {
        /// <summary>
        /// Eindeutige ID des TourStops (Primärschlüssel).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Fremdschlüssel auf die Tour, zu der dieser Stop gehört.
        /// </summary>
        public int TourId { get; set; }

        /// <summary>
        /// Navigationseigenschaft zur Tour.
        /// </summary>
        public Tour? Tour { get; set; }

        /// <summary>
        /// Fremdschlüssel auf die Station, die in dieser Tour besucht wird.
        /// </summary>
        public int StationId { get; set; }

        /// <summary>
        /// Navigationseigenschaft zur Station.
        /// </summary>
        public Station? Station { get; set; }

        /// <summary>
        /// Reihenfolge in der Tour (1 = erster Stop, 2 = zweiter Stop, ...).
        /// </summary>
        public int Order { get; set; }
    }
}
