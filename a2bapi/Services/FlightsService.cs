using a2bapi.Data;
using a2bapi.Dtos.Flights;
using a2bapi.Models;
using Microsoft.EntityFrameworkCore;


namespace a2bapi.Services
{
    public interface IFlightsService
    {
        Task<List<SavedFlightDto>> GetSavedFlightsAsync(int userId);
        Task<int> SaveFlightAsync(int userId, SaveFlightRequest request);
        Task<int> DeleteSavedFlightAsync(int userId, int flightId);
    }

    public class FlightsService : IFlightsService
    {
        private readonly AppDbContext _db;

        public FlightsService(AppDbContext db)
        {
            _db = db;
        }


        public async Task<List<SavedFlightDto>> GetSavedFlightsAsync(int userId)
        {
            var flights = await _db.SavedFlights
                .Where(f => f.UserId == userId)
                .Select(f => new SavedFlightDto
                {
                    Id = f.Id,
                    FlightName = f.FlightName,
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,
                    FlightDate = f.FlightDate,
                    DepartureIata = f.DepartureIata,
                    ArrivalIata = f.ArrivalIata,
                    Price = f.Price
                })
                .ToListAsync();

            return flights;
        }

        public async Task<int> SaveFlightAsync(int userId, SaveFlightRequest request)
        {
            var exists = await _db.SavedFlights.AnyAsync(f =>
                f.UserId == userId &&
                f.FlightName == request.FlightName &&
                f.DepartureTime == request.DepartureTime &&
                f.ArrivalTime == request.ArrivalTime &&
                f.FlightDate == request.FlightDate &&
                f.DepartureIata == request.DepartureIata &&
                f.ArrivalIata == request.ArrivalIata
            );

            if (exists)
            {
                throw new InvalidOperationException("Flight already saved.");
            }

            var entity = new SavedFlight
            {
                UserId = userId,
                FlightName = request.FlightName,
                DepartureTime = request.DepartureTime,
                ArrivalTime = request.ArrivalTime,
                FlightDate = request.FlightDate,
                DepartureIata = request.DepartureIata,
                ArrivalIata = request.ArrivalIata,
                Price = request.Price,
            };

            _db.SavedFlights.Add(entity);
            await _db.SaveChangesAsync();

            return entity.Id;
        }


        public async Task<int> DeleteSavedFlightAsync(int userId, int flightId)
        {
            var flight = await _db.SavedFlights.FirstOrDefaultAsync(f => f.Id == flightId && f.UserId == userId);

            if (flight == null)
            {
                throw new Exception("Flight not found.");
            }

            _db.SavedFlights.Remove(flight);
            await _db.SaveChangesAsync();

            return flight.Id;
        }
    }
}
