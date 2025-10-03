using Duende.IdentityModel;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityService.Api.Services
{

    public class CustomProfileService : ProfileService<ApplicationUser>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory) : base(userManager, claimsFactory)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            var principal = await _userManager.CreateSecurityTokenAsync(user);

            //var claims = principal.Claims.ToList();

            //// Add custom claims from ApplicationUser
            //claims.Add(new Claim("user_id", user.Id.ToString()));
            //claims.Add(new Claim(JwtClaimTypes.GivenName, user.FirstName ?? string.Empty));
            //claims.Add(new Claim(JwtClaimTypes.FamilyName, user.LastName ?? string.Empty));

            // Add roles
          //  var roles = await _userManager.GetRolesAsync(user);
         //   claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));

           // context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            context.IsActive = user != null && user.LockoutEnabled == false;
        }
    }
}
