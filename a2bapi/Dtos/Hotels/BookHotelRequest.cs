using System.ComponentModel.DataAnnotations;

namespace a2bapi.Dtos.Hotels
{
    public class BookHotelRequest
    {
        [Required]
        public string HotelName { get; set; } = "";

        [Required]
        public decimal HotelDistance { get; set; }

        [Required]
        public string HotelIataCode { get; set; } = "";

        [Required]
        public string HotelCountryCode { get; set; } = "";

        [Required]
        public int HotelRating { get; set; }

        [Required]
        public decimal TotalCost { get; set; }

        [Required]
        public int NumNights { get; set; }
    }
}