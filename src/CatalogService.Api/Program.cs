using CatalogService.Api;
using CatalogService.Api.Data;
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

var app = builder.Build();


// --- Data Seeding ---
await CatalogDbSeeder.EnsureSeedDataAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
 

app.MapControllers();

app.Run();
