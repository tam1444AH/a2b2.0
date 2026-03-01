using System.Text.Json.Serialization;

namespace a2bapi.Dtos.Auth
{
    public class AmadeusTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = "";

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
