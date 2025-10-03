using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Api.Models.Entities
{
    public class Product : BaseEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CompareAtPrice { get; set; }
        public string Brand { get; set; }
        public bool IsFeatured { get; set; }
        public int StockQuantity { get; set; } // Managed by Inventory service, mirrored here for display
        public bool IsAvailable => StockQuantity > 0; // Derived status

        // Navigation properties
        public Category Category { get; set; }
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
        public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    }
}
