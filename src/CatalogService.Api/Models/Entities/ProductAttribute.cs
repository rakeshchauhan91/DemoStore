
using System.Numerics;

namespace CatalogService.Api.Models.Entities
{
    public class ProductAttribute: BaseEntity<long>
    {
        public long Id { get; set; }
        public int AttributeId { get; set; }
        public Guid ProductId { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }

        // Navigation property
        public Product Product { get; set; }
    }
}