namespace a2bapi.Dtos.Flights
{
    public class FlightSearchResultDto
    {
        public string FlightDate { get; set; } = "";
        public string FlightStatus { get; set; } = "";
        public string AirlineName { get; set; } = "";
        public string FlightNumber { get; set; } = "";
        public string DepatureIata { get; set; } = "";
        public string ArrivalIata { get; set; } = "";
        public DateTimeOffset? DepartureScheduled { get; set; }
        public DateTimeOffset? ArrivalScheduled { get; set; }
    }
}
