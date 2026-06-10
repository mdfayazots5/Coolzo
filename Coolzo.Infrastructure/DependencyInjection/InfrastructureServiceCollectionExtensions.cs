using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Infrastructure.Identity;
using Coolzo.Infrastructure.Logging;
using Coolzo.Infrastructure.Security;
using Coolzo.Infrastructure.Services;
using Coolzo.Infrastructure.Storage;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Coolzo.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptionsSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = jwtOptionsSection.Get<JwtOptions>() ?? new JwtOptions();
        var signingKey = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);

        services.Configure<JwtOptions>(jwtOptionsSection);
        services.AddHttpContextAccessor();

        services.AddSingleton<ICurrentDateTime, SystemCurrentDateTime>();
        services.AddSingleton<IApplicationEnvironment, HostApplicationEnvironment>();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IBookingReferenceGenerator, BookingReferenceGenerator>();
        services.AddSingleton<IServiceRequestNumberGenerator, ServiceRequestNumberGenerator>();
        services.AddSingleton<IJobCardNumberGenerator, JobCardNumberGenerator>();
        services.AddSingleton<IQuotationNumberGenerator, QuotationNumberGenerator>();
        services.AddSingleton<IInvoiceNumberGenerator, InvoiceNumberGenerator>();
        services.AddSingleton<IReceiptNumberGenerator, ReceiptNumberGenerator>();
        services.AddSingleton<ISupportTicketNumberGenerator, SupportTicketNumberGenerator>();
        services.AddSingleton<IGapPhaseAReferenceGenerator, GapPhaseAReferenceGenerator>();
        services.AddSingleton<IInstallationLifecycleReferenceGenerator, InstallationLifecycleReferenceGenerator>();
        services.AddScoped<IJobAttachmentStorageService, LocalJobAttachmentStorageService>();

        var objectStorageSection = configuration.GetSection(ObjectStorageOptions.SectionName);
        services.Configure<ObjectStorageOptions>(objectStorageSection);
        var objectStorageOptions = objectStorageSection.Get<ObjectStorageOptions>() ?? new ObjectStorageOptions();

        if (string.Equals(objectStorageOptions.Provider, ObjectStorageOptions.S3Provider, StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IAmazonS3>(_ => CreateS3Client(objectStorageOptions.S3));
            services.AddScoped<IObjectStorageService, S3ObjectStorageService>();
        }
        else
        {
            services.AddScoped<IObjectStorageService, FileSystemObjectStorageService>();
        }

        services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKey),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization(options =>
        {
            foreach (var permission in PermissionNames.All)
            {
                options.AddPolicy(permission, policy => policy.RequireClaim(CustomClaimTypes.Permission, permission));
            }

            options.AddPolicy("AdminOnly", policy => policy.RequireRole(RoleNames.SuperAdmin, RoleNames.Admin));
        });

        return services;
    }

    /// <summary>
    /// Builds an S3 client for an S3-compatible vendor. For Cloudflare R2 (and MinIO/Backblaze) set
    /// S3.ServiceUrl to the account endpoint and keep ForcePathStyle = true; for native AWS S3 leave
    /// ServiceUrl empty so the SDK resolves the regional endpoint. Credentials come from configuration
    /// / environment (ObjectStorage:S3:AccessKey, :SecretKey) — never hardcoded.
    /// </summary>
    private static IAmazonS3 CreateS3Client(S3StorageOptions s3Options)
    {
        var config = new AmazonS3Config
        {
            ForcePathStyle = s3Options.ForcePathStyle,
            AuthenticationRegion = s3Options.Region
        };

        if (!string.IsNullOrWhiteSpace(s3Options.ServiceUrl))
        {
            config.ServiceURL = s3Options.ServiceUrl;
        }
        else
        {
            config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(s3Options.Region);
        }

        var credentials = new BasicAWSCredentials(s3Options.AccessKey, s3Options.SecretKey);
        return new AmazonS3Client(credentials, config);
    }
}
