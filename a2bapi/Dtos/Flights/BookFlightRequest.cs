using System.ComponentModel.DataAnnotations;

namespace a2bapi.Dtos.Flights
{
    public class BookFlightRequest
    {
        [Required]
        public string FlightName { get; set; } = "";

        [Required]
        public string FlightDate { get; set; } = "";

        [Required]
        public string DepartureIata { get; set; } = "";

        [Required]
        public string ArrivalIata { get; set; } = "";

        [Required]
        public string DepartureTime { get; set; } = "";

        [Required]
        public string ArrivalTime { get; set; } = "";

        [Range(1, int.MaxValue)]
        public int NumTickets { get; set; }

        [Range(typeof(decimal), "0.01", "999999999")]
        public decimal TotalCost { get; set; }
    }
}
