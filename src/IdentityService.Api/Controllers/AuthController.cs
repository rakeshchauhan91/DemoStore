using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Extensions;  
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
 
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace IdentityService.Api.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEventPublisher _eventBus;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration; // <-- NEW: Inject IConfiguration

        public AuthController(
           UserManager<ApplicationUser> userManager,
           SignInManager<ApplicationUser> signInManager,
           IEventPublisher eventBus,
           ITokenService tokenService,
           IConfiguration configuration) // <-- NEW: Inject configuration
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _eventBus = eventBus;
            _tokenService = tokenService;
            _configuration = configuration; // <-- Store configuration
        }
         

        /// POST /api/auth/register
        [HttpPost("register")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] Models.DTO.RegisterRequestDto request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = true // For MVP, we skip email verification
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Add role
            await _userManager.AddToRoleAsync(user, request.Role);

            // Publish UserRegistered event
            await _eventBus.PublishAsync(new Models.Events.UserRegistered(user.Id.ToString(), user.Email!, user.FirstName));

            return StatusCode(201, new { UserId = user.Id, Email = user.Email });
        }

        /// POST /api/auth/login
        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] Models.DTO.LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized("Invalid credentials.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
                return Unauthorized("Invalid credentials.");

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // --- Simplified Token Generation for API (Resource Owner Password Flow Abstraction) ---
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, user.Id.ToString()),
                new Claim(JwtClaimTypes.Name, user.UserName!),
                new Claim(JwtClaimTypes.Email, user.Email!),
            };
            claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));

            // The full OIDC flow would involve an external client calling the /connect/token endpoint.
            // This is a custom service-side abstraction to meet the endpoint requirement.
            var accessToken = await GenerateAccessTokenAsync(user, claims);
            //var refreshToken = await _userManager.CreateSecurityTokenAsync(user); // Using identity token creation for simplicity
            var refreshTokenValue = $"{user.Id}_{Guid.NewGuid():N}";
            // Publish UserLoggedIn event
            await _eventBus.PublishAsync(new Models.Events.UserLoggedInEvent(user.Id.ToString(), user.Email!));

            
            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue, // Mocked refresh token value
                ExpiresIn = 3600 // Example
            });
        }

        // --- Helper to manually generate a JWT for the API response (simplifying Duende) ---
        private async Task<string> GenerateAccessTokenAsync(ApplicationUser user, IEnumerable<Claim> claims)
        {
            string issuerUri = _configuration["IdentityServer:IssuerUri"]
                               ?? $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

            var token = new Token(IdentityServerConstants.TokenTypes.AccessToken)
            {
                ClientId = "ecommerce.webapp",
                Lifetime = 3600,
                AccessTokenType = AccessTokenType.Jwt,
                Claims = claims.ToList(),
                Audiences = { "ecommerce.api" },
                Issuer = issuerUri 
            };

            // Using Duende's ITokenService to build and sign the token
            var tokenCreationService = HttpContext.RequestServices.GetRequiredService<ITokenCreationService>();
            var jwtToken = await tokenCreationService.CreateTokenAsync(token);

            return jwtToken;
        }

        // POST /api/auth/refresh-token
        [HttpPost("refresh-token")]
        // In a real application, this would call Duende's /connect/token endpoint with grant_type=refresh_token.
        // For a pure API implementation, you would need to implement refresh token validation logic here.
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            // Placeholder: In a microservice, a dedicated service validates the old refresh token
            // and issues a new access token and refresh token pair.

            // To be implemented:
            // 1. Validate request.RefreshToken against stored tokens (if using persistent grants).
            // 2. Find the user associated with the token.
            // 3. Revoke the old token.
            // 4. Generate new AccessToken and RefreshToken.

            return Ok(new { Message = "Refresh token logic pending (typically handled by a dedicated Duende /connect/token call)." });
        }

        // POST /api/auth/forgot-password
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            // Placeholder: Send password reset link/code to the user's email.
            return Ok(new { Message = $"Password reset link sent to {request.Email}." });
        }

        // POST /api/auth/reset-password
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            // Placeholder: Validate token/code, find user, update password, publish PasswordChanged event.
            // await _eventBus.PublishAsync(new PasswordChanged(userId));
            return Ok(new { Message = "Password has been successfully reset." });
        }
    }
}
