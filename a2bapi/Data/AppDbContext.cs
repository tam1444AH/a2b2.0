using Microsoft.EntityFrameworkCore;
using a2bapi.Models;

namespace a2bapi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<SavedFlight> SavedFlights => Set<SavedFlight>();
        public DbSet<SavedHotel> SavedHotels => Set<SavedHotel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(x => x.Email).IsUnique();
                e.Property(x => x.Email).HasMaxLength(256);
            });

            modelBuilder.Entity<SavedFlight>(e =>
            {
                e.Property(x => x.FlightName).HasColumnName("flight_name");
                e.Property(x => x.DepartureTime).HasColumnName("departure_time");
                e.Property(x => x.ArrivalTime).HasColumnName("arrival_time");
                e.Property(x => x.FlightDate).HasColumnName("flight_date");
                e.Property(x => x.DepartureIata).HasColumnName("departure_iata");
                e.Property(x => x.ArrivalIata).HasColumnName("arrival_iata");
                e.Property(x => x.Price).HasColumnName("price");
                e.Property(x => x.UserId).HasColumnName("user_id");


                e.HasOne(x => x.User)
                .WithMany(u => u.SavedFlights)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SavedHotel>(e =>
            {
                e.Property(x => x.HotelName).HasColumnName("hotel_name");
                e.Property(x => x.HotelDistance).HasColumnName("hotel_distance");
                e.Property(x => x.HotelIataCode).HasColumnName("iata_code");
                e.Property(x => x.HotelCountryCode).HasColumnName("country_code");
                e.Property(x => x.HotelRating).HasColumnName("hotel_rating");
                e.Property(x => x.Price).HasColumnName("price");
                e.Property(x => x.UserId).HasColumnName("user_id");


                e.HasOne(x => x.User)
                .WithMany(u => u.SavedHotels)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            });

        }

    }
}
