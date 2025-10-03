using FluentValidation;
 

namespace IdentityService.Api.Models.DTO
{

    public class SocialLoginRequestDto
    {  
        public string AccessToken { get; set; }
    }
    public class SocialLoginRequestValidator : AbstractValidator<SocialLoginRequestDto>
    {
        public SocialLoginRequestValidator()
        {
            RuleFor(x => x.AccessToken).NotEmpty();
            
        }
    }
}
