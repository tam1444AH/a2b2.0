using System.ComponentModel.DataAnnotations;    

namespace a2bapi.Dtos.Hotels
{
    public class SaveHotelRequest
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
        public decimal Price { get; set; }

    }
}
