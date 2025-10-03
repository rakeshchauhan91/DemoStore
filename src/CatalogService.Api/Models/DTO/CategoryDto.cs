namespace CatalogService.Api.Models.DTO
{
    public record CategoryDto(
       Guid Id,
       string Name,
       string Description,
       Guid? ParentCategoryId,
       string ImageUrl,
       bool IsActive,
       List<CategoryDto> SubCategories
   );
}
