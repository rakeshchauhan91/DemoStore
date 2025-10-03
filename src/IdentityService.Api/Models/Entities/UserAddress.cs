using Infrastructure.Defaults;
using System.ComponentModel.DataAnnotations;

namespace IdentityService.Api.Models.Entities
{
    public class UserAddress : BaseEntity<Guid>
    {
        [Key]
        public Guid Id { get; set; } 

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(200)]
        public string AddressLine1 { get; set; }

        [MaxLength(200)]
        public string AddressLine2 { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(100)]
        public string State { get; set; }

        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string Country { get; set; }

        public bool IsDefault { get; set; } = false;
    }
}
