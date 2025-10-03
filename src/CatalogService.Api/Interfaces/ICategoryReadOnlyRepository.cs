using CatalogService.Api.Models.Entities;
 

namespace CatalogService.Api.Interfaces
{
    public interface ICategoryReadOnlyRepository : IReadRepository<Category, Guid>
    {
        Task<List<Category>> GetAllWithSubCategoriesAsync(CancellationToken cancellationToken = default);
    }
}
