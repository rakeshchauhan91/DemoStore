using FluentValidation;

namespace IdentityService.Api.Models.DTO
{
    public record LoginRequestDto(string Email, string Password);
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
