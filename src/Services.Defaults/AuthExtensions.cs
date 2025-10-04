using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
namespace Services.Defaults
{
    public static class AuthExtensions
    {
        // Parameter for the required scope of the microservice being configured (e.g., "catalog.fullaccess")
        public static IServiceCollection AddDefaultJwtBearerAuthentication(
            this IServiceCollection services,
            IConfiguration configuration,
            string[] requiredApiScope)
        {
            var identityServerSection = configuration.GetSection("Identity");
            var authority = identityServerSection.GetRequiredValue("Url");
            var audience = identityServerSection.GetRequiredValue("Audience");
            var symmetricKeySecret = identityServerSection.GetRequiredValue("SymmetricKeySecret");

            // 1. Create the Symmetric Key for validation
            var validationKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(symmetricKeySecret));

            // 2. Configure JWT Bearer Authentication
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    // Set the Authority to validate the 'issuer' claim in the token
                    options.Authority = authority;
                    options.RequireHttpsMetadata = false;
                    options.Audience = audience;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // Explicitly set the symmetric key for validation
                        IssuerSigningKey = validationKey,
                        ValidateIssuerSigningKey = true,

                        // Validate the issuer claim
                        ValidateIssuer = true,

                        // Disable audience validation in favor of granular scope checking (optional, but common)
                        ValidateAudience = false
                    };
                });

            // 3. Configure Authorization Policy based on the provided requiredApiScope
            services.AddAuthorization(options =>
            {
                // Create a policy named "ApiScopePolicy" that all controllers can use
                options.AddPolicy("ApiScopePolicy", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    // Enforce that the token MUST contain the scope claim matching the parameter
                    foreach (var scope in requiredApiScope)
                    {
                        policy.RequireClaim("scope", scope);
                    }
                });
            });

            return services;
        }
    }
}
