using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
 
namespace Services.Defaults
{
    public static partial class Extensions
    {
        // New class to hold the scope data
        private sealed record ScopesArgument(string[] Scopes);

        private sealed class AuthorizeCheckOperationFilter(ScopesArgument scopesArgument) : IOperationFilter
        {
            // The scopes are now accessed via the record property
            private readonly string[] _scopes = scopesArgument.Scopes;

            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

                if (!metadata.OfType<IAuthorizeData>().Any())
                {
                    return;
                }
                // ... (rest of the Apply method remains the same)

                var oAuthScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                };

                operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                // Use the inner _scopes array
                [ oAuthScheme ] = _scopes
            }
        };
            }
        }
        public static IHostApplicationBuilder AddDefaultOpenApi(this IHostApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;

            var openApi = configuration.GetSection("OpenApi");

            if (!openApi.Exists())
            {
                return builder;
            }

            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {

                var document = openApi.GetRequiredSection("Document");

                var version = document.GetRequiredValue("Version") ?? "v1";

                options.SwaggerDoc(version, new OpenApiInfo
                {
                    Title = document.GetRequiredValue("Title"),
                    Version = version,
                    Description = document.GetRequiredValue("Description")
                });

                var identitySection = configuration.GetSection("Identity");

                if (!identitySection.Exists())
                {
                    // No identity section, so no authentication open api definition
                    return;
                }
                var identityUrlExternal = identitySection.GetRequiredValue("Url");
                var scopes = identitySection.GetRequiredSection("Scopes").GetChildren().ToDictionary(p => p.Key, p => p.Value);

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows()
                    {
                        // TODO: Change this to use Authorization Code flow with PKCE
                        Implicit = new OpenApiOAuthFlow()
                        {
                            AuthorizationUrl = new Uri($"{identityUrlExternal}/connect/authorize"),
                            TokenUrl = new Uri($"{identityUrlExternal}/connect/token"),
                            Scopes = scopes,
                        }
                    }
                });
                var scopesArray = scopes.Keys.ToArray();
                var filterArgument = new ScopesArgument(scopesArray);

                // Pass the single wrapped object as the argument
                options.OperationFilter<AuthorizeCheckOperationFilter>(filterArgument);


            });

            return builder;
        }

    }
}
