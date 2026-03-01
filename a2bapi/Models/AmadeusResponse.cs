using System.Text.Json.Serialization;

namespace a2bapi.Models
{
    public class AmadeusResponseObject
    {
        [JsonPropertyName("data")]
        public List<Hotel>? Data { get; set; }
    }
    public class Hotel
    {
        [JsonPropertyName("chainCode")]
        public string? ChainCode { get; set; }
        [JsonPropertyName("iataCode")]
        public string? IataCode { get; set; }
        [JsonPropertyName("dupeId")]
        public long DupeId { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("hotelId")]
        public string? HotelId { get; set; }
        [JsonPropertyName("geoCode")]
        public GeoCode? GeoCode { get; set; }
        [JsonPropertyName("address")]
        public Address? Address { get; set; }
        [JsonPropertyName("distance")]
        public Distance? Distance { get; set; }
        [JsonPropertyName("rating")]
        public int Rating { get; set; }
        [JsonPropertyName("lastUpdate")]
        public string? LastUpdate { get; set; }
    }

    public class GeoCode
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }

    public class Address
    {
        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }
    }

    public class Distance
    {
        [JsonPropertyName("value")]
        public double Value { get; set; }
        [JsonPropertyName("unit")]
        public string? Unit { get; set; }
    }
}