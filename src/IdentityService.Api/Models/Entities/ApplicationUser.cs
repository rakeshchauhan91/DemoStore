using Infrastructure.Defaults;
using Microsoft.AspNetCore.Identity;


namespace IdentityService.Api.Models.Entities
{
    public abstract class BaseIdentityUser<TKey> : IdentityUser<TKey>, IEntity<TKey>
      where TKey : IEquatable<TKey>
    {
        // BaseEntity properties
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? LastSyncDt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public Guid ExternalId { get; set; } = Guid.NewGuid();

        // IEntity implementation
        TKey IEntity<TKey>.Id
        {
            get => base.Id;
            set => base.Id = value;
        }
    }

    public class ApplicationUser : BaseIdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateTime? LastLoginAt { get; set; }
        public bool IsEmailVerified { get; set; } = false;


        // Navigation properties
        public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        // IEntity implementation
        public Guid Id { get => base.Id; set => base.Id = value; }

    }

}
