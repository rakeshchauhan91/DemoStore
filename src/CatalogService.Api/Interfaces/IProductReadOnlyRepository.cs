using CatalogService.Api.Models.Entities;
using Infrastructure.Defaults.Persistence;

namespace CatalogService.Api.Interfaces
{

    public interface IProductReadOnlyRepository : IReadRepository<Product, Guid>
    {
        Task<Product?> GetProductBySkuAsync(string sku, CancellationToken cancellationToken = default);
        Task<List<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default);
        Task<PaginatedResult<Product>> SearchProductsPaginatedAsync(string query, PaginationRequest request, CancellationToken cancellationToken = default);
    }
}
