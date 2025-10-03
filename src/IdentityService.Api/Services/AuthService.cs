
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IdentityService.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IdentityDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEventBus _eventBus; // Event bus for publishing events

        public AuthService(IdentityDbContext context, IConfiguration configuration, IEventBus eventBus)
        {
            _context = context;
            _configuration = configuration;
            _eventBus = eventBus;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string role = "Customer")
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new ApplicationException("User with this email already exists.");
            }

            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // Hash password
                IsEmailVerified = false, // In MVP, might skip email verification or add a simple flow
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.UserRoles.Add(new UserRole { User = user, Role = role });
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            await _eventBus.PublishAsync(new UserRegisteredEvent { UserId = user.UserId, Email = user.Email, CreatedAt = user.CreatedAt });

            return new AuthResponse
            {
                AccessToken = token,
                Expiration = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpirationMinutes"]))
            };
            // Refresh token logic would be more complex and usually involves a separate table
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                               .Include(u => u.Roles)
                               .SingleOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new ApplicationException("Invalid credentials.");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            await _eventBus.PublishAsync(new UserLoggedInEvent { UserId = user.UserId, Email = user.Email, LoggedInAt = user.LastLoginAt.Value });

            return new AuthResponse
            {
                AccessToken = token,
                Expiration = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpirationMinutes"]))
            };
        }

        public Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            // For MVP, this might be a placeholder.
            // A full implementation requires storing refresh tokens securely in the database,
            // associating them with users, and revoking them.
            throw new NotImplementedException("Refresh token logic not implemented for MVP.");
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                // For security, don't confirm if email exists
                return;
            }

            // Generate a password reset token (e.g., GUID or cryptographically strong random string)
            // Store it in the database with an expiration date.
            // Send an email to the user with a link containing this token.
            Console.WriteLine($"[MVP] Password reset requested for {email}. Send email with reset token.");
        }

        public async Task ResetPasswordAsync(string email, string token, string newPassword)
        {
            // Validate the token against what's stored in the database.
            // If valid and not expired, update the user's password hash.
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null /* || !IsTokenValid(token) */)
            {
                throw new ApplicationException("Invalid or expired reset token.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            // Invalidate the reset token
            await _context.SaveChangesAsync();
            await _eventBus.PublishAsync(new PasswordChangedEvent { UserId = user.UserId, Email = user.Email, ChangedAt = DateTime.UtcNow });
        }


        private string GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationMinutes"])),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Helper for social login (MVP placeholder)
        public async Task<AuthResponse> SocialLoginAsync(string provider, string accessToken)
        {
            // In a real scenario, this would involve calling the social provider's API
            // to validate the accessToken and retrieve user info.
            // Then, either find the user by their social ID or register them if new.

            // For MVP, let's just simulate a login
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == $"{provider}_user@example.com");
            if (user == null)
            {
                // Simulate registration
                user = new User
                {
                    Email = $"{provider}_user@example.com",
                    FirstName = $"{provider} User",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Random password
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                _context.UserRoles.Add(new UserRole { User = user, Role = "Customer" });
                await _context.SaveChangesAsync();
                await _eventBus.PublishAsync(new UserRegisteredEvent { UserId = user.UserId, Email = user.Email, CreatedAt = user.CreatedAt });
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _eventBus.PublishAsync(new UserLoggedInEvent { UserId = user.UserId, Email = user.Email, LoggedInAt = user.LastLoginAt.Value });

            var token = GenerateJwtToken(user);
            return new AuthResponse
            {
                AccessToken = token,
                Expiration = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpirationMinutes"]))
            };
        }
    }
}
