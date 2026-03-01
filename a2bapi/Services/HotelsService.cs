using a2bapi.Data;
using a2bapi.Dtos.Hotels;
using a2bapi.Models;
using Microsoft.EntityFrameworkCore;

namespace a2bapi.Services
{
    public interface IHotelsService
    {
        Task<List<SavedHotelDto>> GetSavedHotelsAsync(int userId);
        Task<int> SaveHotelAsync(int userId, SaveHotelRequest request);
        Task<int> DeleteSavedHotelAsync(int userId, int hotelId);

    }
    public class HotelsService : IHotelsService
    {
        private readonly AppDbContext _db;

        public HotelsService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<SavedHotelDto>> GetSavedHotelsAsync(int userId)
        {
            var hotels = await _db.SavedHotels
                .Where(h => h.UserId == userId)
                .Select(h => new SavedHotelDto
                {
                    Id = h.Id,
                    HotelName = h.HotelName,
                    HotelDistance = h.HotelDistance,
                    HotelIataCode = h.HotelIataCode,
                    HotelCountryCode = h.HotelCountryCode,
                    HotelRating = h.HotelRating,
                    Price = h.Price,
                })
                .ToListAsync();

            return hotels;
        }

        public async Task<int> SaveHotelAsync(int userId, SaveHotelRequest request)
        {
            var exists = await _db.SavedHotels.AnyAsync(f =>
                f.UserId == userId &&
                f.HotelName == request.HotelName &&
                f.HotelDistance == request.HotelDistance &&
                f.HotelIataCode == request.HotelIataCode &&
                f.HotelCountryCode == request.HotelCountryCode &&
                f.HotelRating == request.HotelRating &&
                f.Price == request.Price
            );

            if (exists)
            {
                throw new InvalidOperationException("Hotel already saved.");
            }

            var entity = new SavedHotel
            {
                UserId = userId,
                HotelName = request.HotelName,
                HotelDistance = request.HotelDistance,
                HotelIataCode = request.HotelIataCode,
                HotelCountryCode = request.HotelCountryCode,
                HotelRating = request.HotelRating,
                Price = request.Price,
            };

            _db.SavedHotels.Add(entity);
            await _db.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<int> DeleteSavedHotelAsync(int userId, int hotelId)
        {
            var hotel = await _db.SavedHotels.FirstOrDefaultAsync(h => h.Id == hotelId && h.UserId == userId);

            if (hotel == null)
            {
                throw new Exception("Hotel not found.");
            }

            _db.SavedHotels.Remove(hotel);
            await _db.SaveChangesAsync();

            return hotel.Id;
        }
    }
}
