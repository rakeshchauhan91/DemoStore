using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET    /api/products (with filtering, sorting, pagination)
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProductSummaryDto>))]
        public async Task<IActionResult> GetProducts(
            [FromQuery] string q = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1) return BadRequest("Page and pageSize must be greater than 0.");

            var (products, totalCount) = await _productService.SearchProductsAsync(q, page, pageSize);

            // Adding pagination metadata to response headers
            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Page-Number", page.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());

            return Ok(products);
        }

        // GET    /api/products/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDetailDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }
            return Ok(product);
        }

        // GET    /api/products/search?q={query}
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProductSummaryDto>))]
        public async Task<IActionResult> SearchProducts([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Delegates to the main GET /api/products for core logic consistency
            return await GetProducts(q, page, pageSize);
        }

        // GET    /api/products/featured
        [HttpGet("featured")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProductSummaryDto>))]
        public async Task<IActionResult> GetFeaturedProducts()
        {
            var products = await _productService.GetFeaturedProductsAsync();
            return Ok(products);
        }

        // GET    /api/products/{id}/variants
        [HttpGet("{id:guid}/variants")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProductVariantDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductVariants(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound($"Product with ID {id} not found.");

            return Ok(product.Variants);
        }
    }
}
