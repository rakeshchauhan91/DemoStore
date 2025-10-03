using CatalogService.Api.Data;
using CatalogService.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
 
using System.Linq.Expressions;


namespace CatalogService.Api.Repositories
{
    public class ProductRepository : GenericRepository<Product, Guid, CatalogDbContext>, IProductReadOnlyRepository
    {
        public ProductRepository(CatalogDbContext context) : base(context)
        {
        }

        public async Task<Product?> GetProductBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);
        }

        public async Task<List<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.IsFeatured && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<PaginatedResult<Product>> SearchProductsPaginatedAsync(
            string query,
            PaginationRequest request,
            CancellationToken cancellationToken = default)
        {
            var searchQuery = _dbSet
                .Where(p => !p.IsDeleted &&
                       (p.Name.Contains(query) ||
                        p.Description.Contains(query) ||
                        p.SKU.Contains(query)));

            return await GetPaginatedAsync(request, searchQuery.Expression as Expression<Func<Product, bool>>, cancellationToken);
        }

        // Example: Override a method if you need custom behavior
        public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // Custom logic: Always include Category when getting by ID
            return await _dbSet
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
    }
}
