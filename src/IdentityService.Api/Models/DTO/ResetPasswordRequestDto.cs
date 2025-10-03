using FluentValidation;
 

namespace IdentityService.Api.Models.DTO
{
    public class ResetPasswordRequestDto
    {
         
        public string Email { get; set; }
    
        public string Token { get; set; }
 
        public string NewPassword { get; set; }
    }
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.NewPassword).NotEmpty();
        }
    }
}
