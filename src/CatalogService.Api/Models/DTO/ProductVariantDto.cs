using FluentValidation;

namespace CatalogService.Api.Models.DTO
{

    public record ProductVariantDto(
        int? Id, Guid? ProductId, string SKU, string Name, decimal Price, string AttributesJson
    );
    public class ProductVariantDtoValidator : AbstractValidator<ProductVariantDto>
    {
        public ProductVariantDtoValidator()
        {
            RuleFor(v => v.Name).NotEmpty().WithMessage("Variant Name is required.");
            RuleFor(v => v.SKU).NotEmpty().MaximumLength(60);
            RuleFor(v => v.Price).GreaterThanOrEqualTo(0).WithMessage("Variant Price must be non-negative.");
        }
    }
}
