namespace a2bapi.Dtos.Hotels
{
    public class SavedHotelDto
    {
        public int Id { get; set; }
        public string HotelName { get; set; } = "";
        public decimal HotelDistance { get; set; }
        public string HotelIataCode { get; set; } = "";
        public string HotelCountryCode { get; set; } = "";
        public int HotelRating { get; set; }
        public decimal Price { get; set; }
    }
}
