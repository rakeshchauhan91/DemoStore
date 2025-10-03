using System.ComponentModel.DataAnnotations;
using System.Data;

namespace IdentityService.Api.Models.DTO
{
    // Event DTOs
    
    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
    }

    // User Profile DTOs
   

    public class UpdateUserProfileRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
    }
    
    public class ForgotPasswordRequest
    {
        public  string Email { get; set; }
    }
    public class RefreshTokenRequest { }
    public class CreateAddressRequest
    {
        [Required]
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string Country { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UpdateAddressRequest : CreateAddressRequest { } // For simplicity, reuse fields
}
