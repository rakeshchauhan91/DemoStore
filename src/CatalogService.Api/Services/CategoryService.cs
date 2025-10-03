using CatalogService.Api.Models.Entities;
using FluentValidation;
using System.Linq.Expressions;

namespace CatalogService.Api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryReadOnlyRepository _categoryReadRepository;
        private readonly IProductReadOnlyRepository _productReadRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateCategoryRequest> _createValidator;

        public CategoryService(
            ICategoryReadOnlyRepository categoryReadRepository,
            IProductReadOnlyRepository productReadRepository,
            IUnitOfWork unitOfWork,
            IValidator<CreateCategoryRequest> createValidator)
        {
            _categoryReadRepository = categoryReadRepository;
            _productReadRepository = productReadRepository;
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
        }

        public async Task<Guid> CreateCategoryAsync(CreateCategoryRequest request)
        {
            await _createValidator.ValidateAndThrowAsync(request); // Throws ValidationException on error

            var categoryRepository = _unitOfWork.GetRepository<Category, Guid>();

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                ParentCategoryId = request.ParentCategoryId,
                ImageUrl = request.ImageUrl,
                IsActive = true
            };

            var createdCategory = await categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return createdCategory.Id;
        }

        public async Task UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request)
        {
            // Assuming an UpdateCategoryRequestValidator exists and is used here.
            var categoryRepository = _unitOfWork.GetRepository<Category, Guid>();

            var category = await categoryRepository.GetByIdAsync(categoryId);
            if (category == null) throw new KeyNotFoundException($"Category ID {categoryId} not found.");

            category.Name = request.Name;
            category.Description = request.Description;
            category.ParentCategoryId = request.ParentCategoryId;
            category.ImageUrl = request.ImageUrl;
            category.IsActive = request.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await categoryRepository.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            var categoryRepository = _unitOfWork.GetRepository<Category, Guid>();

            // Check for children or associated products before deleting
            if (await categoryRepository.ExistsAsync(c => c.ParentCategoryId == categoryId))
            {
                throw new InvalidOperationException("Cannot delete category with sub-categories.");
            }

            if (await _productReadRepository.ExistsAsync(p => p.Id == categoryId))
            {
                throw new InvalidOperationException("Cannot delete category that has products assigned.");
            }

            await categoryRepository.DeleteAsync(categoryId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            // Fetch all categories with sub-categories for tree structure
            var categories = await _categoryReadRepository.GetAllWithSubCategoriesAsync();

            // Simple mapping to DTOs without building the tree for brevity. 
            // A more complete implementation would build a hierarchical DTO structure.
            return categories.Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.ParentCategoryId,
                c.ImageUrl,
                c.IsActive,
                // Assuming c.SubCategories is loaded via include
                c.SubCategories.Select(sub => new CategoryDto(sub.Id, sub.Name, sub.Description, sub.ParentCategoryId, sub.ImageUrl, sub.IsActive, new List<CategoryDto>())).ToList()
            )).ToList();
        }

        public async Task<List<ProductSummaryDto>> GetProductsByCategoryIdAsync(Guid categoryId)
        {
            // 1. Define the filter expression
            Expression<Func<Product, bool>> filter = p => p.CategoryId == categoryId && p.IsActive;

            // 2. Create the SearchCriteria object to include the filter and the required navigation properties
            var criteria = new SearchCriteria<Product>
            {
                Filter = filter,
                IncludeProperties = "Images"  // Specify the include property
            };

            // 3. Execute FindAsync using SearchCriteria
            // Note: IReadOnlyRepository's FindAsync with SearchCriteria is used here.
            var products = await _productReadRepository.FindAsync(criteria);

            // 4. Map Domain Entities to DTOs
            return products.Select(p => new ProductSummaryDto(
                p.Id,
                p.Name,
                p.SKU,
                p.BasePrice,
                p.CompareAtPrice,
                p.Brand,
                p.IsFeatured,
                p.IsAvailable,
                p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
            )).ToList();
        }
        
    }
}
