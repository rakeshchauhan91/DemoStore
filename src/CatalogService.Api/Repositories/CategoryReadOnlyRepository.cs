using CatalogService.Api.Data;
using CatalogService.Api.Models.Entities;
using Infrastructure.Defaults.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Repositories
{
    /// <summary>
    /// Implementation of ICategoryReadOnlyRepository, extending the base GenericRepository
    /// for common read operations and adding category-specific methods.
    /// </summary>
    // Assuming GenericRepository<TEntity, TKey> implements IReadRepository<TEntity, TKey>
    public class CategoryReadOnlyRepository : GenericRepository<Category, Guid, CatalogDbContext>, ICategoryReadOnlyRepository
    {
        private readonly CatalogDbContext _context;

        public CategoryReadOnlyRepository(CatalogDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all categories and eagerly loads their direct sub-categories.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of all Category entities, including sub-categories.</returns>
        public Task<List<Category>> GetAllWithSubCategoriesAsync(CancellationToken cancellationToken = default)
        {
            // Eagerly load the direct SubCategories collection
            // We use AsNoTracking() as this is a read-only operation
            return _context.Categories
                .AsNoTracking()
                .Include(c => c.SubCategories.Where(sub => sub.IsActive)) // Filter active sub-categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }
    }
}