using System.Text;
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
}
