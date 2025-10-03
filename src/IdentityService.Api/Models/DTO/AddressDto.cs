using FluentValidation;

namespace IdentityService.Api.Models.DTO
{
    public record AddressDto(
         Guid? Id, // Nullable for POST
         string AddressLine1,
         string AddressLine2,
         string City,
         string State,
         string PostalCode,
         string Country,
         bool IsDefault
     );

    public class AddressDtoValidator : AbstractValidator<AddressDto>
    {
        public AddressDtoValidator()
        {
            RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(250);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        }
    }

    // Simple mapper for brevity
    public static class AddressMapper
    {
        public static AddressDto ToDto(UserAddress entity) => new(
            entity.Id, entity.AddressLine1, entity.AddressLine2, entity.City, entity.State, entity.PostalCode, entity.Country, entity.IsDefault);

        public static UserAddress ToEntity(AddressDto dto, Guid userId, Guid? id = null) => new()
        {
            Id = id ?? dto.Id ?? Guid.Empty, // Use the provided ID or 0 for new
            UserId = userId,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            IsDefault = dto.IsDefault
        };
    }
}
