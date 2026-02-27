using a2bapi.Models;
using a2bapi.Dtos.Flights;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace a2bapi.Services
{
    public interface IAviationStackService
    {
        Task<List<FlightSearchResultDto>> SearchFlightsAsync(string departureIata, string arrivalIata, CancellationToken ct = default);
    }
    public class AviationStackService : IAviationStackService
    {
        private readonly HttpClient _http;
        private readonly AviationStackSettings _settings;

        public AviationStackService(HttpClient http, IOptions<AviationStackSettings> settings)
        {
            _http = http;
            _settings = settings.Value;
        }

        public async Task<List<FlightSearchResultDto>> SearchFlightsAsync(string departureIata, string arrivalIata, CancellationToken ct = default)
        {

            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                throw new InvalidOperationException("Aviation Stack API key is not configured.");
            }

            var from = departureIata.Trim().ToUpperInvariant();
            var to = arrivalIata.Trim().ToUpperInvariant();

            var url = $"flights?access_key={Uri.EscapeDataString(_settings.ApiKey)}&dep_iata={Uri.EscapeDataString(from)}&arr_iata={Uri.EscapeDataString(to)}";

            var response = await _http.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"AviationStack API request failed with status code {(int)response.StatusCode}. Response body: {body}");
            }

            var json = await response.Content.ReadFromJsonAsync<FlightsResponse>(cancellationToken: ct);
            var data = json?.Data ?? new List<FlightData>();

            return data.Select(f => new FlightSearchResultDto
            {
                FlightDate = f.FlightDate ?? "",
                FlightStatus = f.FlightStatus ?? "",
                AirlineName = f.Airline?.Name ?? "",
                FlightNumber = f.Flight?.Number ?? "",
                DepatureIata = f.Departure?.Iata ?? "",
                ArrivalIata = f.Arrival?.Iata ?? "",
                DepartureScheduled = TryParseOffset(f.Departure?.Scheduled),
                ArrivalScheduled = TryParseOffset(f.Arrival?.Scheduled),
            }).ToList();
        }

        private static DateTimeOffset? TryParseOffset(string? value)
            => DateTimeOffset.TryParse(value, out var result) ? result : null;
    }
}
