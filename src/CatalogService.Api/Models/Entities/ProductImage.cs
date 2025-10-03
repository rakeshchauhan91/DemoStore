


namespace CatalogService.Api.Models.Entities
{
    public class ProductImage : BaseEntity<long>
    {
        public long Id { get; set; }
        public int ImageId { get; set; }
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }

        // Navigation property
        public Product Product { get; set; }
    }
}