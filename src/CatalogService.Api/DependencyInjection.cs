
using CatalogService.Api.Data;
using CatalogService.Api.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Defaults.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
 
using Microsoft.EntityFrameworkCore;
using System.Reflection;


namespace CatalogService.Api
{
    public static class DependencyInjection
    {
        public static void AddAPIDepedencies(this IHostApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
     ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


            var Configuration = builder.Configuration;
            // Database Context
            builder.Services.AddDbContext<CatalogDbContext>(options =>
                options.UseSqlServer(connectionString));

            // --- 2. IdentityServer (Duende) & JWT Bearer Authentication ---
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = builder.Configuration["Identity:Authority"]; // e.g., "https://localhost:5001"
                    options.Audience = "catalog_api"; // The API Resource name defined in Duende IS
                    options.RequireHttpsMetadata = builder.Environment.IsProduction();
                    // options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" }; // for FAPI compliance
                });

            builder.Services.AddAuthorization(options =>
            {
                // Define policy for admin-only endpoints
                options.AddPolicy("CanWriteCatalog", policy =>
                    policy.RequireClaim("scope", "catalog_api.write")
                          .RequireRole("Admin"));
            });

            builder.Services.AddScoped<IProductReadOnlyRepository, ProductRepository>();
            builder.Services.AddScoped<ICategoryReadOnlyRepository, CategoryReadOnlyRepository>();
             

            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork<CatalogDbContext>>();


            builder.Services.AddScoped<IValidator<CreateProductRequest>, CreateProductRequestValidator>();

            // builder.Services.AddSingleton<IEventPublisher, AzureServiceBusEventPublisher>(); // Mock Event Bus
            builder.Services.AddSingleton<IEventPublisher, MockEventBus>(); // Mock Event Bus
            
            // --- FluentValidation Configuration ---
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
