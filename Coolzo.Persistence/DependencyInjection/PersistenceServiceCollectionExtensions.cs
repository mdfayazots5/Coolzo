using Coolzo.Application.Common.Interfaces;
using Coolzo.Persistence.Context;
using Coolzo.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Coolzo.Persistence.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=Coolzo;Trusted_Connection=True;TrustServerCertificate=True;";

        services.AddDbContext<CoolzoDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IBookingLookupRepository, BookingLookupRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ICustomerAppRepository, CustomerAppRepository>();
        services.AddScoped<IAnalyticsReadRepository, AnalyticsReadRepository>();
        services.AddScoped<IAmcRepository, AmcRepository>();
        services.AddScoped<IBillingRepository, BillingRepository>();
        services.AddScoped<IFieldLookupRepository, FieldLookupRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IGapPhaseARepository, GapPhaseARepository>();
        services.AddScoped<IGapPhaseERepository, GapPhaseERepository>();
        services.AddScoped<IInstallationLifecycleRepository, InstallationLifecycleRepository>();
        services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
        services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
        services.AddScoped<ITechnicianRepository, TechnicianRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IUserPasswordHistoryRepository, UserPasswordHistoryRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<IAdminConfigurationRepository, AdminConfigurationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");

        return services;
    }
}
