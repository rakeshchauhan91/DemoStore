namespace CatalogService.Api.Models.DTO
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public string Description { get; set; }
        public string Brand { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsAvailable { get; set; }
        public int CategoryId { get; set; }
        public string PrimaryImageUrl { get; set; }
    }
    public record TagDto(
    int TagId, string Name
);
   
 
    public record ProductAttributeDto(
        long? AttributeId, string AttributeName, string AttributeValue
    );
 
}
