using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
 

namespace IdentityService.Api.Data.Seed
{

    //dotnet ef migrations add InitialIdentityServerConfigurationMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/Configuration
    //dotnet ef migrations add InitialIdentityServerPersistedGrantMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrant
    //dotnet ef migrations add InitialApplicationDbMigration -c AppIdentityDbContext -o Data/Migrations/ApplicationDb
    public static class SeedData
    {
        public static async Task EnsureSeedData(WebApplication app)
        {
            using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
            var configContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            // 1. Apply Migrations
            await context.Database.MigrateAsync();
            await configContext.Database.MigrateAsync();

            // 2. Seed Duende IdentityServer Configuration (Clients, Resources)
            if (!configContext.Clients.Any())
            {
                foreach (var client in IdentityConfig.Clients)
                    configContext.Clients.Add(client.ToEntity());
                await configContext.SaveChangesAsync();
            }

            if (!configContext.IdentityResources.Any())
            {
                foreach (var resource in IdentityConfig.IdentityResources)
                    configContext.IdentityResources.Add(resource.ToEntity());
                await configContext.SaveChangesAsync();
            }

            if (!configContext.ApiScopes.Any())
            {
                foreach (var idcscope in IdentityConfig.ApiScopes)
                    configContext.ApiScopes.Add(idcscope.ToEntity());
                await configContext.SaveChangesAsync();
            }

            if (!configContext.ApiResources.Any())
            {
                foreach (var resource in IdentityConfig.ApiResources)
                    configContext.ApiResources.Add(resource.ToEntity());
                await configContext.SaveChangesAsync();
            }

            // 3. Seed Roles
            var roles = new[] { "Admin", "Customer", "Vendor" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }

            // 4. Seed Users
            if (await userManager.FindByEmailAsync("admin@example.com") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "P@ssw0rd123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    // Add a sample address for the admin
                    context.UserAddresses.Add(new UserAddress
                    {
                        UserId = adminUser.Id,
                        AddressLine1 = "123 Admin St",
                        AddressLine2= "line 2 of address",
                        City = "Metropolis",
                        State = "CA",
                        PostalCode = "90210",
                        Country = "USA",
                        IsDefault = true
                    });
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
