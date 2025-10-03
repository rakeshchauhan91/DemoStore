namespace CatalogService.Api.Models.DTO
{
    public record ProductSummaryDto(
        Guid Id,
        string Name,
        string SKU,
        decimal BasePrice,
        decimal? CompareAtPrice,
        string Brand,
        bool IsFeatured,
        bool IsAvailable,
        string PrimaryImageUrl
    );
}
