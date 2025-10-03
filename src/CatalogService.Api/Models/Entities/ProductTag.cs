using System.Numerics;

namespace CatalogService.Api.Models.Entities
{
    public class ProductTag : BaseEntity<long>
    {
        public long Id { get; set; }
        public Guid ProductId { get; set; }
        public int TagId { get; set; }

        // Navigation properties
        public Product Product { get; set; }
        public Tag Tag { get; set; }
    }
}
