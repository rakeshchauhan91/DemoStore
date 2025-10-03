using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Api.Models.DTO
{

    public record CreateCategoryRequest(
         string Name,
         string Description,
         Guid? ParentCategoryId,
         string ImageUrl
     );
    public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
    {
        public CreateCategoryRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(500);
            RuleFor(x => x.ParentCategoryId).Must(id => id.GetValueOrDefault() != Guid.Empty).WithMessage("ParentCategoryId must be non-negative or null.");
        }
    }
    public record UpdatePriceRequest(
      decimal NewPrice
  );
   

    public record UpdateCategoryRequest(
        string Name,
        string Description,
        Guid? ParentCategoryId,
        string ImageUrl,
        bool IsActive
    );

    
}
