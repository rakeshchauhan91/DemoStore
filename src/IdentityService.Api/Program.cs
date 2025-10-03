using IdentityService.Api;
using IdentityService.Api.Data.Seed;
using Services.Defaults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var configurationBuilder = new ConfigurationBuilder()
.AddJsonFile($"appsettings.{environment}.json", true, true)
.AddEnvironmentVariables()
.Build();

builder.AddDefaultHealthChecks();

builder.AddDefaultOpenApi();

builder.AddAPIDepedencies();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// --- Data Seeding ---
await SeedData.EnsureSeedData(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting(); // Ensure routing is before auth

// Duende IdentityServer Middleware
app.UseIdentityServer();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
