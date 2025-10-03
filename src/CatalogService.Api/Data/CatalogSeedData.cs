using CatalogService.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Api.Data
{
    public static class CatalogDbSeeder
    {
        private static readonly Guid ProductS21Guid = Guid.Parse("A0000000-0000-0000-0000-000000000001");
        private static readonly Guid ProductXPS13Guid = Guid.Parse("A0000000-0000-0000-0000-000000000002");
        private static readonly Guid ProductTSHIRTGuid = Guid.Parse("A0000000-0000-0000-0000-000000000003");

        public static async Task EnsureSeedDataAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

                // Idempotency check
                if (await context.Products.AnyAsync())
                {
                    return;
                }

                Console.WriteLine("Seeding Catalog Service database...");

                // 1. Seed Categories (Guid PK - NO IDENTITY)
                var seededCategories = await SeedCategoriesAsync(context);
                await context.SaveChangesAsync();

                // 2. Seed Products (Guid PK - NO IDENTITY)
                var seededProducts = await SeedProductsAsync(context, seededCategories);
                await context.SaveChangesAsync();

                // 3. Seed Tags (Int PK - IDENTITY)
                var seededTags = await SeedTagsAsync(context);
                await context.SaveChangesAsync(); // COMMIT 3: Get generated Tag IDs

                // 4. Seed Nested Entities (Long/Int PKs - ALL IDENTITY)
                await SeedNestedEntitiesAsync(context, seededProducts, seededTags);
                await context.SaveChangesAsync();

                Console.WriteLine("Catalog Service database seeding complete.");
            }
        }

        // --- 1. CATEGORIES (PK: Guid, Not Identity) ---
        private static async Task<List<Category>> SeedCategoriesAsync(CatalogDbContext context)
        {
            var categories = new List<Category>
            {
                // Explicit Guid PKs for easier FK referencing, no IDENTITY_INSERT issue
                new Category { Id = Guid.NewGuid(), Name = "Electronics", Description = "Devices and Gadgets", ImageUrl = "/img/cat/electronics.jpg", IsActive = true, CreatedAt = DateTime.UtcNow }, // Category 1
                new Category { Id = Guid.NewGuid(), Name = "Smartphones", Description = "Latest Mobile Phones", ImageUrl = "/img/cat/phones.jpg", IsActive = true, CreatedAt = DateTime.UtcNow }, // Category 2
                new Category { Id = Guid.NewGuid(), Name = "Laptops", Description = "Portable Computers", ImageUrl = "/img/cat/laptops.jpg", IsActive = true, CreatedAt = DateTime.UtcNow }, // Category 3
                new Category { Id = Guid.NewGuid(), Name = "Apparel", Description = "Clothing and accessories", ImageUrl = "/img/cat/apparel.jpg", IsActive = true, CreatedAt = DateTime.UtcNow }, // Category 4
                new Category { Id = Guid.NewGuid(), Name = "T-Shirts", Description = "Casual Wear", ImageUrl = "/img/cat/tshirts.jpg", IsActive = true, CreatedAt = DateTime.UtcNow } // Category 5
            };

            // Set ParentCategoryIds after initial list creation
            categories[1].ParentCategoryId = categories[0].Id; // Smartphones -> Electronics
            categories[2].ParentCategoryId = categories[0].Id; // Laptops -> Electronics
            categories[4].ParentCategoryId = categories[3].Id; // T-Shirts -> Apparel

            await context.Categories.AddRangeAsync(categories);
            return categories;
        }

        // --- 2. PRODUCTS (PK: Guid, Not Identity) ---
        private static async Task<List<Product>> SeedProductsAsync(CatalogDbContext context, List<Category> categories)
        {
            var smartphoneCatId = categories.First(c => c.Name == "Smartphones").Id;
            var laptopCatId = categories.First(c => c.Name == "Laptops").Id;
            var tshirtCatId = categories.First(c => c.Name == "T-Shirts").Id;

            var products = new List<Product>
            {
                new Product { Id = ProductS21Guid, CategoryId = smartphoneCatId, SKU = "P-SAM-S21", Name = "Samsung Galaxy S21", Description = "Flagship Android Phone", BasePrice = 799.99M, CompareAtPrice = 899.99M, Brand = "Samsung", IsFeatured = true, IsActive = true, StockQuantity = 15, CreatedAt = DateTime.UtcNow },
                new Product { Id = ProductXPS13Guid, CategoryId = laptopCatId, SKU = "P-DELL-XP13", Name = "Dell XPS 13", Description = "Premium Ultrabook Laptop", BasePrice = 1299.00M, CompareAtPrice = null, Brand = "Dell", IsFeatured = true, IsActive = true, StockQuantity = 5, CreatedAt = DateTime.UtcNow },
                new Product { Id = ProductTSHIRTGuid, CategoryId = tshirtCatId, SKU = "P-TSH-BLUE-XL", Name = "Classic Blue T-Shirt", Description = "100% Cotton, XL Size", BasePrice = 25.00M, CompareAtPrice = 30.00M, Brand = "GenericWear", IsFeatured = false, IsActive = true, StockQuantity = 50, CreatedAt = DateTime.UtcNow }
            };
            await context.Products.AddRangeAsync(products);
            return products;
        }

        // --- 3. TAGS (PK: Int, IDENTITY) ---
        private static async Task<List<Tag>> SeedTagsAsync(CatalogDbContext context)
        {
            // IMPORTANT: TagId is Identity (int PK), so we DO NOT set it.
            var tags = new List<Tag>
            {
                new Tag { Name = "Sale" },
                new Tag { Name = "New Arrival" },
                new Tag { Name = "Android" }
            };
            await context.Tags.AddRangeAsync(tags);
            return tags; // Return list to get IDs after save
        }

        // --- 4. NESTED ENTITIES (All are Identity PKs: long/int) ---
        private static async Task SeedNestedEntitiesAsync(CatalogDbContext context, List<Product> products, List<Tag> tags)
        {
            // --- A. ProductImages (PK: long, IDENTITY) ---
            var images = new List<ProductImage>
            {
                // Id (PK) is IDENTITY, so we DO NOT set it. ProductId (FK) is GUID.
                new ProductImage { ProductId = ProductS21Guid, ImageUrl = "/img/products/s21-main.jpg", IsPrimary = true, DisplayOrder = 1 },
                new ProductImage { ProductId = ProductS21Guid, ImageUrl = "/img/products/s21-side.jpg", IsPrimary = false, DisplayOrder = 2 },
                new ProductImage { ProductId = ProductXPS13Guid, ImageUrl = "/img/products/xps13-main.jpg", IsPrimary = true, DisplayOrder = 1 },
                new ProductImage { ProductId = ProductTSHIRTGuid, ImageUrl = "/img/products/tshirt-blue.jpg", IsPrimary = true, DisplayOrder = 1 }
            };
            await context.ProductImages.AddRangeAsync(images);

            // --- B. ProductVariants (PK: int, IDENTITY) ---
            var variants = new List<ProductVariant>
            {
                // Id (PK) is IDENTITY, so we DO NOT set it.
                new ProductVariant { ProductId = ProductS21Guid, SKU = "V-S21-BLK-128", Name = "Black - 128GB", Price = 799.99M, Attributes = "{ \"color\": \"Black\", \"storage\": \"128GB\" }" },
                new ProductVariant { ProductId = ProductS21Guid, SKU = "V-S21-SIL-256", Name = "Silver - 256GB", Price = 899.99M, Attributes = "{ \"color\": \"Silver\", \"storage\": \"256GB\" }" }
            };
            await context.ProductVariants.AddRangeAsync(variants);

            // --- C. ProductAttributes (PK: long, IDENTITY) ---
            var attributes = new List<ProductAttribute>
            {
                // Id (PK) is IDENTITY, so we DO NOT set it.
                new ProductAttribute { ProductId = ProductXPS13Guid, AttributeName = "Processor", AttributeValue = "Intel Core i7" },
                new ProductAttribute { ProductId = ProductXPS13Guid, AttributeName = "RAM", AttributeValue = "16 GB" }
            };
            await context.ProductAttributes.AddRangeAsync(attributes);

            // --- D. ProductTags (PK: long, IDENTITY) ---
            var saleTagId = tags.First(t => t.Name == "Sale").Id; // TagId is NOT the PK, but the FK value
            var newArrivalTagId = tags.First(t => t.Name == "New Arrival").Id;
            var androidTagId = tags.First(t => t.Name == "Android").Id;

            var productTags = new List<ProductTag>
            {
                // Id (PK) is IDENTITY, so we DO NOT set it. ProductId (FK) is GUID, TagId (FK) is INT.
                new ProductTag { ProductId = ProductS21Guid, TagId = saleTagId },
                new ProductTag { ProductId = ProductS21Guid, TagId = androidTagId },
                new ProductTag { ProductId = ProductXPS13Guid, TagId = newArrivalTagId }
            };
            await context.ProductTags.AddRangeAsync(productTags);
        }
    }
}
