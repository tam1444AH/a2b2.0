using a2bapi.Data;
using a2bapi.Dtos.Auth;
using a2bapi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace a2bapi.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> SignupAsync(SignupRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IJwtService _jwt;

        public AuthService(AppDbContext db, IPasswordHasher<User> hasher, IJwtService jwt)
        {
            _db = db;
            _hasher = hasher;
            _jwt = jwt;
        }

        public async Task<AuthResponse> SignupAsync(SignupRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var exists = await _db.Users.AnyAsync(u => u.Email == email);
            if (exists)
            {
                throw new InvalidOperationException("Email already in use.");
            }

            var user = new User { Email = email };
            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return new AuthResponse { Token = _jwt.CreateToken(user.Email, user.Id) };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            return new AuthResponse { Token = _jwt.CreateToken(user.Email, user.Id) };
        }
    }
}