using System.Text.Json.Serialization;

namespace a2bapi.Models
{
    public class FlightsResponse
    {
        [JsonPropertyName("pagination")]
        public Pagination? Pagination { get; set; }

        [JsonPropertyName("data")]
        public List<FlightData>? Data { get; set; }
    }

    public class Pagination
    {
        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class FlightData
    {
        [JsonPropertyName("flight_date")]
        public string? FlightDate { get; set; }

        [JsonPropertyName("flight_status")]
        public string? FlightStatus { get; set; }

        [JsonPropertyName("departure")]
        public FlightDetails? Departure { get; set; }

        [JsonPropertyName("arrival")]
        public FlightDetails? Arrival { get; set; }

        [JsonPropertyName("airline")]
        public AirlineInfo? Airline { get; set; }

        [JsonPropertyName("flight")]
        public FlightInfo? Flight { get; set; }
    }

    public class FlightDetails
    {
        [JsonPropertyName("airport")]
        public string? Airport { get; set; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        [JsonPropertyName("iata")]
        public string? Iata { get; set; }

        [JsonPropertyName("icao")]
        public string? Icao { get; set; }

        [JsonPropertyName("terminal")]
        public string? Terminal { get; set; }

        [JsonPropertyName("gate")]
        public string? Gate { get; set; }

        [JsonPropertyName("delay")]
        public int? Delay { get; set; }

        [JsonPropertyName("scheduled")]
        public string? Scheduled { get; set; }

        [JsonPropertyName("estimated")]
        public string? Estimated { get; set; }

        [JsonPropertyName("actual")]
        public string? Actual { get; set; }

        [JsonPropertyName("estimated_runway")]
        public string? EstimatedRunway { get; set; }

        [JsonPropertyName("actual_runway")]
        public string? ActualRunway { get; set; }
    }

    public class AirlineInfo
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("iata")]
        public string? Iata { get; set; }

        [JsonPropertyName("icao")]
        public string? Icao { get; set; }
    }

    public class FlightInfo
    {
        [JsonPropertyName("number")]
        public string? Number { get; set; }

        [JsonPropertyName("iata")]
        public string? Iata { get; set; }

        [JsonPropertyName("icao")]
        public string? Icao { get; set; }

        [JsonPropertyName("codeshared")]
        public CodeSharedInfo? Codeshared { get; set; }
    }

    public class CodeSharedInfo
    {
        [JsonPropertyName("airline_name")]
        public string? AirlineName { get; set; }

        [JsonPropertyName("airline_iata")]
        public string? AirlineIata { get; set; }

        [JsonPropertyName("airline_icao")]
        public string? AirlineIcao { get; set; }

        [JsonPropertyName("flight_number")]
        public string? FlightNumber { get; set; }

        [JsonPropertyName("flight_iata")]
        public string? FlightIata { get; set; }

        [JsonPropertyName("flight_icao")]
        public string? FlightIcao { get; set; }
    }
}