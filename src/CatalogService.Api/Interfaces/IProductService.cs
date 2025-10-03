using CatalogService.Api.Models.DTO;

namespace CatalogService.Api.Interfaces
{
    public interface IProductService
    {
        // CRUD Operations
        Task<Guid> CreateProductAsync(CreateProductRequest request);
        Task UpdateProductAsync(Guid productId, UpdateProductRequest request);
        Task DeleteProductAsync(Guid productId);
        Task<ProductDetailDto> GetProductByIdAsync(Guid productId);

        // Price Management
        Task UpdateProductPriceAsync(Guid productId, UpdatePriceRequest request);

        // Query/Search Operations
        Task<(List<ProductSummaryDto> Products, int TotalCount)> SearchProductsAsync(string query, int page, int pageSize);
        Task<List<ProductSummaryDto>> GetFeaturedProductsAsync();

        // Image Management
        Task AddImageToProductAsync(Guid productId, ProductImageDto imageDto);
        Task DeleteImageAsync(int imageId);

        // Variant Management
        Task<int> AddVariantToProductAsync(Guid productId, ProductVariantDto variantDto);
        Task UpdateVariantAsync(int variantId, ProductVariantDto variantDto);
    }
}
