 

namespace CatalogService.Api.Interfaces
{
    public interface ICategoryService
    {
        Task<Guid> CreateCategoryAsync(CreateCategoryRequest request);
        Task UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request);
        Task DeleteCategoryAsync(Guid categoryId);
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<List<ProductSummaryDto>> GetProductsByCategoryIdAsync(Guid categoryId);
    }
}
