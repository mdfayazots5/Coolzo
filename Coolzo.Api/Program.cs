using Coolzo.Api.Extensions;
using Coolzo.Api.HealthChecks;
using Coolzo.Api.Middleware;
using Coolzo.Application.DependencyInjection;
using Coolzo.Infrastructure.DependencyInjection;
using Coolzo.Infrastructure.Storage;
using Coolzo.Persistence.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddPresentation();

// CMS object storage runs exclusively on Cloudflare R2. Render's filesystem is ephemeral, so writing
// uploaded images / published snapshots to disk loses them on the next redeploy. Fail fast at startup
// if the R2 settings are incomplete instead of silently accepting an unusable configuration.
ObjectStorageConfigurationGuard.Validate(builder.Configuration);

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
                "https://admin.coolzo.in",
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

app.UseRouting();
// CORS must run BEFORE the static-file middleware: static files short-circuit the pipeline, so
// without this ordering the cross-origin Web portal fetch of /cms/snapshot-latest.json (and other
// CMS objects) returns 200 but carries no Access-Control-Allow-Origin header and the browser blocks it.
app.UseCors("FrontendPolicy");

app.UseStaticFiles();

// CMS objects (images + snapshot) are served directly from the Cloudflare R2 bucket's public URL,
// so no local static-file mapping for object storage is required.

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
