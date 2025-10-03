namespace CatalogService.Api.Models.DTO
{
    public record ProductImageDto(
    long? ImageId, string ImageUrl, bool IsPrimary, int DisplayOrder);

}
