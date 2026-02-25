using System.Text.Json.Serialization;

namespace a2bapi.Models
{
    public class HotelApiResponse
    {
        [JsonPropertyName("data")]
        public List<Hotel>? Data { get; set; }
    }
}