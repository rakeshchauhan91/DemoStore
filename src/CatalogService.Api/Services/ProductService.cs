using CatalogService.Api.Models.Entities;
using CatalogService.Api.Models.Events;
using FluentValidation;
 
using Infrastructure.Defaults.Services;

namespace CatalogService.Api.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductReadOnlyRepository _productReadRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventPublisher _eventBus;
        private readonly IValidator<CreateProductRequest> _createValidator;

        public ProductService(
            IProductReadOnlyRepository productReadRepository,
            IUnitOfWork unitOfWork,
            IEventPublisher eventBus,
            IValidator<CreateProductRequest> createValidator)
        {
            _productReadRepository = productReadRepository;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _createValidator = createValidator;
        }
        public async Task<(List<ProductSummaryDto> Products, int TotalCount)> SearchProductsAsync(string query, int page, int pageSize)
        {
            // 1. Prepare Pagination Request
            var paginationRequest = new PaginationRequest(page, pageSize);

            // 2. Execute Search via Read-Only Repository
            // This method handles the filtering (based on 'query'), sorting, and pagination internally.
            var paginatedResult = await _productReadRepository.SearchProductsPaginatedAsync(query, paginationRequest);

            // 3. Map Domain Entities to DTOs
            var productDtos = paginatedResult.Items.Select(p => new ProductSummaryDto(
                p.Id,
                p.Name,
                p.SKU,
                p.BasePrice,
                p.CompareAtPrice,
                p.Brand,
                p.IsFeatured,
                p.IsAvailable,
                // Assumes images are included by the repository's search method
                p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
            )).ToList();

            // 4. Return results with total count
            return (productDtos, paginatedResult.TotalCount);
        }

        public async Task<List<ProductSummaryDto>> GetFeaturedProductsAsync()
        {
            // 1. Execute Query via Read-Only Repository
            var products = await _productReadRepository.GetFeaturedProductsAsync();

            // 2. Map Domain Entities to DTOs
            var productDtos = products.Select(p => new ProductSummaryDto(
                p.Id,
                p.Name,
                p.SKU,
                p.BasePrice,
                p.CompareAtPrice,
                p.Brand,
                p.IsFeatured,
                p.IsAvailable,
                // Assumes images are included by the repository method for the primary image
                p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
            )).ToList();

            // 3. Return results
            return productDtos;
        }
        public async Task<Guid> CreateProductAsync(CreateProductRequest request)
        {
            // 1. Validation
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Optional: Check SKU uniqueness
            if (await _productReadRepository.ExistsAsync(p => p.SKU == request.SKU))
            {
                throw new InvalidOperationException($"Product with SKU '{request.SKU}' already exists.");
            }

            // 2. Mapping Request to Domain Entity
            var product = new Product
            {
                Id = Guid.NewGuid(), // PK property
                CategoryId = request.CategoryId,
                SKU = request.SKU,
                Name = request.Name,
                BasePrice = request.BasePrice,
              //  IsActive = true,
                // ... nested entity mapping ...
            };

            // 3. Persist (Using IUnitOfWork and IWriteRepository)
            var productRepository = _unitOfWork.GetRepository<Product, Guid>();
            await productRepository.AddAsync(product);

            // Ensure atomicity
            await _unitOfWork.SaveChangesAsync();

            // 4. Publish Event
            var integrationEvent = new ProductCreatedEvent(product.Id, product.SKU, product.Name);
            await _eventBus.PublishAsync(integrationEvent);

            return product.Id;
        }

        public async Task UpdateProductAsync(Guid productId, UpdateProductRequest request)
        {
            // Note: Update logic is often performed within a transaction
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var productRepository = _unitOfWork.GetRepository<Product, Guid>();

                // Fetch the product for update tracking
                var product = await productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {productId} not found.");
                }

                // Check for price change BEFORE applying other updates
                if (product.BasePrice != request.BasePrice)
                {
                    var oldPrice = product.BasePrice;
                    product.BasePrice = request.BasePrice;

                    // Publish Price Change Event (should be done after SaveChanges in a successful transaction)
                    var priceEvent = new ProductPriceChanged(productId, request.BasePrice, oldPrice);
                    await _eventBus.PublishAsync(priceEvent); // Outbox Pattern is better here
                }

                // 2. Apply Updates
                product.Name = request.Name;
                product.Description = request.Description;
                product.IsActive = request.IsActive;
                product.UpdatedAt = DateTime.UtcNow;
                // ... nested entity update/sync logic ...

                // 3. Persist
                await productRepository.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // If successful, publish the generic updated event
                await _eventBus.PublishAsync(new ProductUpdated(product.Id, product.Name));
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw; // Re-throw the exception
            }
        }

        public async Task<ProductDetailDto> GetProductByIdAsync(Guid productId)
        {
            // Read-only path, no transaction needed, use IProductReadOnlyRepository
            // Use GetByIdAsync with Includes for all required navigation properties
            var product = await _productReadRepository.GetByIdAsync(
                productId,
                includeProperties: "Images,Variants,Attributes,ProductTags.Tag" // Specify includes
            );

            if (product == null) return null;

            // Manual Mapping to ProductDetailDto
            return new ProductDetailDto(
                product.Id,
                product.Name,
                product.SKU,
                product.BasePrice,
                product.CompareAtPrice,
                product.Description,
                product.Brand,
                product.IsFeatured,
                product.IsActive,
                product.IsAvailable,
                product.CategoryId,
                product.Images.Select(i => new ProductImageDto(i.ImageId, i.ImageUrl, i.IsPrimary, i.DisplayOrder)).ToList(),
                product.Attributes.Select(a => new ProductAttributeDto(a.AttributeId, a.AttributeName, a.AttributeValue)).ToList(),
                product.Variants.Select(v => new ProductVariantDto(v.Id,v.ProductId, v.SKU, v.Name, v.Price, v.Attributes)).ToList(),
                product.ProductTags.Select(pt => new TagDto(pt.TagId, pt.Tag.Name)).ToList()
            );
        }


        public async Task DeleteProductAsync(Guid productId)
        {
            var productRepository = _unitOfWork.GetRepository<Product, Guid>();

            // 1. Check existence first
            var product = await productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            // 2. Delete
            await productRepository.DeleteAsync(product);

            // 3. Commit Transaction
            await _unitOfWork.SaveChangesAsync();

            // 4. Publish Event
            await _eventBus.PublishAsync(new ProductDeletedEvent(productId));
        }


        public async Task UpdateProductPriceAsync(Guid productId, UpdatePriceRequest request)
        {
            var productRepository = _unitOfWork.GetRepository<Product, Guid>();

            // 1. Get Product
            var product = await productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            if (request.NewPrice <= 0)
            {
                throw new InvalidOperationException("New price must be greater than zero.");
            }

            // 2. Check for change and update
            if (product.BasePrice != request.NewPrice)
            {
                var oldPrice = product.BasePrice;
                product.BasePrice = request.NewPrice;
                product.UpdatedAt = DateTime.UtcNow;

                // 3. Persist
                await productRepository.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                // 4. Publish Event
                var integrationEvent = new ProductPriceChanged(productId, request.NewPrice, oldPrice);
                await _eventBus.PublishAsync(integrationEvent);
            }
            // If price is the same, do nothing (no save, no event).
        }

        public async Task AddImageToProductAsync(Guid productId, ProductImageDto imageDto)
        {
            // We need to access the Product entity's repository, which requires Product to be loaded 
            // with its Images collection for correct tracking.
            var productRepository = _unitOfWork.GetRepository<Product, Guid>();

            // Load product with existing images
            var product = await productRepository.GetByIdAsync(productId, includeProperties: "Images");
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            // Optional: Ensure only one image is primary (or unset the old one)
            if (imageDto.IsPrimary)
            {
                var currentPrimary = product.Images.FirstOrDefault(i => i.IsPrimary);
                if (currentPrimary != null)
                {
                    currentPrimary.IsPrimary = false;
                }
            }

            // Create new image entity
            var newImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageDto.ImageUrl,
                IsPrimary = imageDto.IsPrimary,
                DisplayOrder = imageDto.DisplayOrder
            };

            product.Images.Add(newImage); // EF Core will track this as an ADD

            await productRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteImageAsync(int imageId)
        {
            // We need a repository for ProductImage (or access it via UnitOfWork)
            var imageRepository = _unitOfWork.GetRepository<ProductImage, long>();

            // 1. Find and Delete
            await imageRepository.DeleteAsync(imageId);

            // 2. Commit
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<int> AddVariantToProductAsync(Guid productId, ProductVariantDto variantDto)
        {
            // We need a repository for ProductVariant (or access it via UnitOfWork)
            var variantRepository = _unitOfWork.GetRepository<ProductVariant, int>();

            // Optional: Check if the base product exists
            if (!await _productReadRepository.ExistsAsync(productId))
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            // Optional: Check SKU uniqueness across variants
            if (await variantRepository.ExistsAsync(v => v.SKU == variantDto.SKU))
            {
                throw new InvalidOperationException($"Product variant with SKU '{variantDto.SKU}' already exists.");
            }

            var newVariant = new ProductVariant
            {
                Id = 0,
                ProductId = productId,
                SKU = variantDto.SKU,
                Name = variantDto.Name,
                Price = variantDto.Price,
                Attributes = variantDto.AttributesJson
            };

            var addedVariant = await variantRepository.AddAsync(newVariant);
            await _unitOfWork.SaveChangesAsync();

            return addedVariant.Id;
        }

        public async Task UpdateVariantAsync(int variantId, ProductVariantDto variantDto)
        {
            var variantRepository = _unitOfWork.GetRepository<ProductVariant, int>();

            // 1. Get existing variant
            var variant = await variantRepository.GetByIdAsync(variantId);
            if (variant == null)
            {
                throw new KeyNotFoundException($"Product variant with ID {variantId} not found.");
            }

            // 2. Apply updates
            variant.SKU = variantDto.SKU;
            variant.Name = variantDto.Name;
            variant.Price = variantDto.Price;
            variant.Attributes = variantDto.AttributesJson;

            // 3. Persist
            await variantRepository.UpdateAsync(variant);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

