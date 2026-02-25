using System.ComponentModel.DataAnnotations;

namespace a2bapi.Models 
{
    public class SavedHotel
    {
        public int Id { get; set; }
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
        [Required]
        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }
}