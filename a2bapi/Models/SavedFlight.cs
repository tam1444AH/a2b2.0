using System.ComponentModel.DataAnnotations;

namespace a2bapi.Models 
{
    public class SavedFlight
    {
        public int Id { get; set; }

        [Required]
        public string FlightName { get; set; } = "";

        [Required]
        public string DepartureTime { get; set; } = "";

        [Required]
        public string ArrivalTime { get; set; } = "";

        [Required]
        public string FlightDate { get; set; } = "";

        [Required]
        public string DepartureIata { get; set; } = "";

        [Required]
        public string ArrivalIata { get; set; } = "";

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }

}
