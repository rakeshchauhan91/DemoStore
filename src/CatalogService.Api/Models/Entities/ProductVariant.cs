


using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Api.Models.Entities
{
    public class ProductVariant : BaseEntity<int>
    {
        public int Id { get; set; }
        public Guid ProductId { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; } // e.g., "Red - Large"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Storing attributes as JSON (flexible schema)
        public string Attributes { get; set; }

        // Navigation property
        public Product Product { get; set; }
    }
}