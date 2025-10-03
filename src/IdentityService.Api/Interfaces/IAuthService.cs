using IdentityService.Api.Models.DTO;

namespace IdentityService.Api.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequestDto request, string role = "Customer");
        Task<AuthResponse> LoginAsync(LoginRequestDto request);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(string email, string token, string newPassword);
    }

}
