namespace CatalogService.Api.Models.DTO
{
    public record ProductDetailDto(
         Guid Id,
         string Name,
         string SKU,
         decimal BasePrice,
         decimal? CompareAtPrice,
         string Description,
         string Brand,
         bool IsFeatured,
         bool IsActive,
         bool IsAvailable,
         Guid CategoryId,
         List<ProductImageDto> Images,
         List<ProductAttributeDto> Attributes,
         List<ProductVariantDto> Variants,
         List<TagDto> Tags
     );
}
