using Infrastructure.Defaults;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IdentityService.Api.Models.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        // IdentityUser already contains: Id (UserId), Email, PasswordHash, PhoneNumber, IsEmailVerified

        // Additional Schema Fields
        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        

        // Navigation properties
        public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        // IEntity implementation
        public Guid Id { get => base.Id; set => base.Id = value; }
    }

}
