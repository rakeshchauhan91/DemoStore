using Infrastructure.Defaults;
using System.ComponentModel.DataAnnotations;

namespace IdentityService.Api.Models.Entities
{
    public class UserRole : BaseEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } // e.g., "Customer", "Admin", "Vendor"
    }

}
