using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityService.Api
{
    public class IdentityConfig
    {
        // Define Identity Resources (claims about the user identity)
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(), // required for OIDC
                new IdentityResources.Profile(), // gives access to claims like name, family_name, etc.
                new IdentityResource("roles", "User role(s)", new[] { JwtClaimTypes.Role }),
                new IdentityResource("userinfo", "User profile data", new[] { "user_id", "first_name", "last_name" })
            };

        // Define API Scopes (permissions/access to resources)
        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("identityservice.api.fullaccess", "Full Access to identityservice APIs"),
                new ApiScope("identityservice.api.read", "Read Access to identityservice APIs")
            };

        // Define API Resources (the APIs being protected)
        public static IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            {
                new ApiResource("Identityservice", "Identityservice")
                {
                    Scopes = { "identityservice.api.fullaccess", "identityservice.api.read" },
                    // Include the custom role claim in the access token
                    UserClaims = { JwtClaimTypes.Role, "user_id", "first_name", "last_name" }
                }
            };

        // Define Clients (applications that request tokens)
        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                // Machine-to-machine client for internal microservices
                new Client
                {
                    ClientId = "client.api",
                    ClientName = "Internal API Client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedScopes = { "ecommerce.api.read" }
                },
                
                // E-commerce Web/Mobile application client (uses Resource Owner Password/Refresh Token flow for MVP)
                new Client
                {
                    ClientId = "identityservice.webapp",
                    ClientName = "identityservice Web/Mobile Client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials, // For login (MVP)
                    // The standard secure flow is Auth Code w/ PKCE, but for a pure API login endpoint:
                    // AllowedGrantTypes = GrantTypes.ResourceOwnerPassword, 
                    ClientSecrets = { new Secret("super_secret_for_client".Sha256()) },
                    AllowOfflineAccess = true, // To enable refresh tokens
                    AccessTokenLifetime = 3600, // 1 hour
                    AbsoluteRefreshTokenLifetime = 2592000, // 30 days
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles",
                        "userinfo",
                        "identityservice.api.fullaccess"
                    }
                }
            };
    }
}
