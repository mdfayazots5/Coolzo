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
        // CMS object storage is Cloudflare R2 (S3-compatible) only. The filesystem provider was removed
        // because Render's disk is ephemeral and silently loses uploaded images and published snapshots
        // on redeploy. ObjectStorageConfigurationGuard validates the R2 settings at startup.
        var objectStorageSection = configuration.GetSection(ObjectStorageOptions.SectionName);
        services.Configure<ObjectStorageOptions>(objectStorageSection);
        var objectStorageOptions = objectStorageSection.Get<ObjectStorageOptions>() ?? new ObjectStorageOptions();

        // CMS client → public coolzo-cms bucket (its own R2 token).
        services.AddSingleton<IAmazonS3>(_ => CreateS3Client(
            objectStorageOptions.S3, objectStorageOptions.S3.AccessKey, objectStorageOptions.S3.SecretKey));
        services.AddScoped<IObjectStorageService, S3ObjectStorageService>();

        // Job/technician media (PII-grade site photos) lives in a SEPARATE PRIVATE bucket with its own
        // least-privilege R2 token — never on ephemeral disk, never public, never sharing the CMS token.
        // Same account endpoint (ServiceUrl/Region/ForcePathStyle), distinct credentials.
        services.AddSingleton(_ => new JobMediaStorageClient(CreateS3Client(
            objectStorageOptions.S3, objectStorageOptions.JobMediaAccessKey, objectStorageOptions.JobMediaSecretKey)));
        services.AddScoped<IJobAttachmentStorageService, R2JobAttachmentStorageService>();

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
    /// Builds an S3 client for an S3-compatible vendor using the supplied credentials. The endpoint
    /// (ServiceUrl/Region/ForcePathStyle) comes from <paramref name="endpointOptions"/>; the access/secret
    /// keys are passed explicitly so the CMS and private job-media buckets can use distinct least-privilege
    /// R2 tokens against the same account endpoint. Credentials come from configuration / environment —
    /// never hardcoded. For Cloudflare R2 keep ForcePathStyle = true; for native AWS S3 leave ServiceUrl
    /// empty so the SDK resolves the regional endpoint.
    /// </summary>
    private static IAmazonS3 CreateS3Client(S3StorageOptions endpointOptions, string accessKey, string secretKey)
    {
        var config = new AmazonS3Config
        {
            ForcePathStyle = endpointOptions.ForcePathStyle,
            AuthenticationRegion = endpointOptions.Region,
            // Cloudflare R2 does not implement the SDK's default flexible-checksum upload mode
            // (STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER), so PutObject fails with
            // "not implemented" on AWSSDK.S3 3.7.40x+. Only compute/validate a checksum when an
            // operation actually requires one — this drops the streaming trailer and lets R2
            // accept uploads. Applies to both the CMS and private job-media R2 clients.
            RequestChecksumCalculation = Amazon.Runtime.RequestChecksumCalculation.WHEN_REQUIRED,
            ResponseChecksumValidation = Amazon.Runtime.ResponseChecksumValidation.WHEN_REQUIRED
        };

        if (!string.IsNullOrWhiteSpace(endpointOptions.ServiceUrl))
        {
            config.ServiceURL = endpointOptions.ServiceUrl;
        }
        else
        {
            config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(endpointOptions.Region);
        }

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        return new AmazonS3Client(credentials, config);
    }
}
