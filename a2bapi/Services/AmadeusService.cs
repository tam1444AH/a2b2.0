using a2bapi.Dtos.Hotels;
using a2bapi.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using a2bapi.Dtos.Auth;
using Microsoft.Extensions.Caching.Memory;

namespace a2bapi.Services
{
    public interface IAmadeusService
    {
        Task<List<HotelSearchResultDto>> SearchHotelsAsync(string to, string dist, string stars);
    }
    public class AmadeusService : IAmadeusService
    {
        private const string TokenCacheKey = "amadeus_access_token";
        private readonly HttpClient _http;
        private readonly AmadeusSettings _settings;
        private readonly IMemoryCache _cache;

        public AmadeusService(HttpClient http, IOptions<AmadeusSettings> settings, IMemoryCache cache)
        {
            _http = http;
            _settings = settings.Value;
            _cache = cache;
        }

        private async Task<string> GetAccessTokenAsync()
        {

            if (_cache.TryGetValue<string>(TokenCacheKey, out var cachedToken))
            {
                return cachedToken!;
            }

            var authBytes = Encoding.ASCII.GetBytes($"{_settings.ApiKey}:{_settings.ApiSecret}");
            var authHeader = Convert.ToBase64String(authBytes);

            using var request = new HttpRequestMessage(HttpMethod.Post, "security/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials"
            });

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to obtain access token from Amadeus API: {error}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<AmadeusTokenResponse>()
                ?? throw new InvalidOperationException("Invalid token response.");

            var secondsToCache = Math.Max(60, tokenResponse.ExpiresIn - 60);

            _cache.Set(
                TokenCacheKey,
                tokenResponse.AccessToken,
                TimeSpan.FromSeconds(secondsToCache)
            );

            return tokenResponse.AccessToken;
        }

        public async Task<List<HotelSearchResultDto>> SearchHotelsAsync(string to, string dist, string stars)
        {

            if (string.IsNullOrWhiteSpace(_settings.ApiKey) || string.IsNullOrWhiteSpace(_settings.ApiSecret))
            {
                throw new InvalidOperationException("Amadeus API credentials are not configured.");
            }

            var arrivalIata = to.Trim().ToUpperInvariant();
            var distance = dist.Trim();
            var starRating = stars.Trim();

            var url = $"reference-data/locations/hotels/by-city?cityCode={Uri.EscapeDataString(arrivalIata)}&radius={Uri.EscapeDataString(distance)}&radiusUnit=MILE&ratings={Uri.EscapeDataString(starRating)}";

            var accessToken = await GetAccessTokenAsync();

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error fetching hotels from Amadeus API: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadFromJsonAsync<AmadeusResponseObject>();
            var data = json?.Data ?? new List<Hotel>();

            return data.Select(h => new HotelSearchResultDto
            {
                HotelName = h.Name ?? "Unknown",
                HotelDistance = h.Distance?.Value ?? 0,
                HotelDistanceUnit = h.Distance?.Unit ?? "N/A",
                HotelCountryCode = h.Address?.CountryCode ?? "N/A",
                HotelRating = h.Rating
            }).ToList();
        }
    }
}
