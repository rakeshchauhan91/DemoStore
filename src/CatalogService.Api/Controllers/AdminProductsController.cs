using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers
{
    [Authorize("CanWriteCatalog")] // Policy defined in Program.cs: scope="catalog_api.write" AND role="Admin"
    [ApiController]
    [Route("api/admin/products")]
    public class AdminProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public AdminProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // POST /api/admin/products
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                var productId = await _productService.CreateProductAsync(request);
                return CreatedAtAction(nameof(ProductsController.GetProductById), "Products", new { id = productId }, new { ProductId = productId });
            }
            catch (FluentValidation.ValidationException ex)
            {
                // Handle validation errors for a clean response
                return BadRequest(ex.Errors);
            }
            catch (InvalidOperationException ex)
            {
                // Handle business logic errors, e.g., duplicate SKU
                return BadRequest(ex.Message);
            }
        }

        // PUT /api/admin/products/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                await _productService.UpdateProductAsync(id, request);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
        }

        // DELETE /api/admin/products/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }

        // PUT /api/admin/products/{id}/price - Custom endpoint for price management
        [HttpPut("{id:guid}/price")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePrice(Guid id, [FromBody] UpdatePriceRequest request)
        {
            try
            {
                await _productService.UpdateProductPriceAsync(id, request);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST /api/admin/products/{id}/images
        [HttpPost("{id:guid}/images")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddProductImage(Guid id, [FromBody] ProductImageDto imageDto)
        {
            await _productService.AddImageToProductAsync(id, imageDto);
            return Ok();
        }

        // DELETE /api/admin/products/images/{imageId}
        [HttpDelete("images/{imageId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteProductImage(int imageId)
        {
            await _productService.DeleteImageAsync(imageId);
            return NoContent();
        }

        // POST /api/admin/products/{id}/variants
        [HttpPost("{id:guid}/variants")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddProductVariant(Guid id, [FromBody] ProductVariantDto variantDto)
        {
            var variantId = await _productService.AddVariantToProductAsync(id, variantDto);
            return CreatedAtAction(null, new { VariantId = variantId });
        }

        // PUT /api/admin/products/variants/{variantId}
        [HttpPut("variants/{variantId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateProductVariant(int variantId, [FromBody] ProductVariantDto variantDto)
        {
            await _productService.UpdateVariantAsync(variantId, variantDto);
            return NoContent();
        }
    }

}