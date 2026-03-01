namespace a2bapi.Dtos.Hotels
{
    public class HotelSearchResultDto
    {
        public string HotelName { get; set; } = "";
        public double HotelDistance { get; set; }
        public string HotelDistanceUnit { get; set; } = "";
        public string HotelCountryCode { get; set; } = "";
        public int HotelRating { get; set; }
        public string HotelIataCode { get; set; } = "";
    }
}
