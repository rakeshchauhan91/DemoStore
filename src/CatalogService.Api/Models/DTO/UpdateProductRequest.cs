using FluentValidation;

namespace CatalogService.Api.Models.DTO
{
    public record UpdateProductRequest(
         int CategoryId,
         string SKU,
         string Name,
         string Description,
         decimal BasePrice,
         decimal? CompareAtPrice,
         string Brand,
         bool IsFeatured,
         bool IsActive,
         List<ProductImageDto> Images,
         List<ProductAttributeDto> Attributes,
         List<ProductVariantDto> Variants,
         List<string> Tags
     );

}
