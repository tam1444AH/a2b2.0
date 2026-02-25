namespace a2bapi.Dtos.Flights
{
    public class SavedFlightDto
    {
        public int Id { get; set; }
        public string FlightName { get; set; } = "";
        public string DepartureTime { get; set; } = "";
        public string ArrivalTime { get; set; } = "";
        public string FlightDate { get; set; } = "";
        public string DepartureIata { get; set; } = "";
        public string ArrivalIata { get; set; } = "";
        public decimal Price { get; set; }

    }
}
