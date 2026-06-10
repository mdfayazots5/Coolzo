using Coolzo.Api.Extensions;
using Coolzo.Api.HealthChecks;
using Coolzo.Api.Middleware;
using Coolzo.Application.DependencyInjection;
using Coolzo.Infrastructure.DependencyInjection;
using Coolzo.Persistence.DependencyInjection;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddPresentation();

builder.Services.AddHealthChecks()
    .AddCheck<ObjectStorageHealthCheck>("object-storage");

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "https://coolzo.in",
                "https://www.coolzo.in",
                "http://127.0.0.1:4200",
                "http://localhost:4200",
                "http://127.0.0.1:3000",
                "http://localhost:3000",
                "http://127.0.0.1:3100",
                "http://localhost:3100",
                "http://127.0.0.1:3101",
                "http://localhost:3101",
                "http://127.0.0.1:5173",
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();

// FileSystem object storage (dev / self-hosted): serve uploaded CMS objects (images + snapshot) from the
// configured RootPath so they remain publicly fetchable even though storage now lives outside wwwroot.
// Prod uses S3/R2 (objects served from the bucket), so this is skipped there.
var objectStorageProvider = builder.Configuration["ObjectStorage:Provider"];
if (string.Equals(objectStorageProvider, "FileSystem", StringComparison.OrdinalIgnoreCase))
{
    var configuredStorageRoot = builder.Configuration["ObjectStorage:RootPath"];
    if (!string.IsNullOrWhiteSpace(configuredStorageRoot))
    {
        var objectStorageRoot = Path.IsPathRooted(configuredStorageRoot)
            ? configuredStorageRoot
            : Path.Combine(app.Environment.ContentRootPath, configuredStorageRoot);
        Directory.CreateDirectory(objectStorageRoot);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(objectStorageRoot),
        });
    }
}

app.UseRouting();
app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
