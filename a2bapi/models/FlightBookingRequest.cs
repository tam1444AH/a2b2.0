using System.ComponentModel.DataAnnotations;

namespace a2bapi.Models 
{
    public class FlightBookingRequest
    {
        public int Id { get; set; }

        [Required]
        public string FlightName { get; set; }

        [Required]
        public string DepartureTime { get; set; }

        [Required]
        public string ArrivalTime { get; set; }

        [Required]
        public string FlightDate { get; set; }

        [Required]
        public string DepartureIata { get; set; }

        [Required]
        public string ArrivalIata { get; set; }

        public int NumTickets { get; set; }

        public double TotalCost { get; set; }

        [Required]
        public int UserId { get; set; }
    }

}
