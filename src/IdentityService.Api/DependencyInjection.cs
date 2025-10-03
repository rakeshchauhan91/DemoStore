using FluentValidation;
using FluentValidation.AspNetCore;
using IdentityService.Api.Services;
using Infrastructure.Defaults.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;


namespace IdentityService.Api
{
    public static class DependencyInjection
    {
        public static void AddAPIDepedencies(this IHostApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


            var Configuration = builder.Configuration;
            // Database Context
            builder.Services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseSqlServer(connectionString));

            // --- Authorization (Protecting the /api/users endpoints) ---
            builder.Services.AddAuthentication()
                .AddLocalApi("Bearer", option =>
                {
                    option.ExpectedScope = "identityservice.api.fullaccess";
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "identityservice.api.fullaccess");
                });
            });


            builder.Services.AddScoped<IUnitOfWork, UnitOfWork<AppIdentityDbContext>>();

            // ASP.NET Core Identity Configuration
            builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.SignIn.RequireConfirmedAccount = false; // For MVP, can be changed later
            })
            .AddUserStore<UserStore<ApplicationUser, IdentityRole<Guid>, AppIdentityDbContext, Guid>>()
            .AddRoleStore<RoleStore<IdentityRole<Guid>, AppIdentityDbContext, Guid>>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders(); // For password reset tokens

            // --- Duende IdentityServer Configuration ---
            var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
            builder.Services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.EmitStaticAudienceClaim = true;
            })
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
                options.EnableTokenCleanup = true;
            })
            .AddInMemoryApiScopes(IdentityConfig.ApiScopes)
            .AddInMemoryClients(IdentityConfig.Clients)
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<CustomProfileService>() // Custom claims for tokens
            .AddDeveloperSigningCredential(); // ONLY for dev/testing, use a certificate in production!


            // builder.Services.AddSingleton<IEventPublisher, AzureServiceBusEventPublisher>(); // Mock Event Bus
            builder.Services.AddSingleton<IEventPublisher, MockEventBus>(); // Mock Event Bus
                                                                            // --- FluentValidation Configuration ---
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
           
            
           
        }
    }
}
