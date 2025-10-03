using FluentValidation;

namespace CatalogService.Api.Models.DTO
{
    public record CreateProductRequest(
    Guid CategoryId,
    string SKU,
    string Name,
    string Description,
    decimal BasePrice,
    decimal? CompareAtPrice,
    string Brand,
    bool IsFeatured,
    List<ProductImageDto> Images,
    List<ProductAttributeDto> Attributes,
    List<ProductVariantDto> Variants,
    List<string> Tags // Tag names
);
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.")
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(v => v.SKU)
                .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.")
                .NotEmpty().WithMessage("SKU is required.");

            RuleFor(v => v.BasePrice)
                .GreaterThan(0).WithMessage("BasePrice must be greater than zero.");

            
            RuleForEach(x => x.Variants).SetValidator(new ProductVariantDtoValidator());
            // RuleFor uniqueness check is better handled in the Service/Repository layer to access the database.
        }
    }
    
}
