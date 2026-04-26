using Coolzo.Application.Common.Interfaces;
using Coolzo.Persistence.Context;
using Coolzo.Persistence.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Coolzo.Persistence.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{
    private const string DefaultLocalDbConnection =
        "Server=(localdb)\\MSSQLLocalDB;Database=Coolzo;Trusted_Connection=True;TrustServerCertificate=True;";

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? DefaultLocalDbConnection;
        connectionString = ResolveConnectionStringForCurrentEnvironment(connectionString, configuration);

        services.AddDbContext<CoolzoDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IBookingLookupRepository, BookingLookupRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ICustomerManagementRepository, CustomerManagementRepository>();
        services.AddScoped<ICustomerAppRepository, CustomerAppRepository>();
        services.AddScoped<IAnalyticsReadRepository, AnalyticsReadRepository>();
        services.AddScoped<IAmcRepository, AmcRepository>();
        services.AddScoped<IBillingRepository, BillingRepository>();
        services.AddScoped<IFieldWorkflowRepository, FieldWorkflowRepository>();
        services.AddScoped<IFieldLookupRepository, FieldLookupRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IGapPhaseARepository, GapPhaseARepository>();
        services.AddScoped<IGapPhaseERepository, GapPhaseERepository>();
        services.AddScoped<IInstallationLifecycleRepository, InstallationLifecycleRepository>();
        services.AddScoped<ISchedulingRepository, SchedulingRepository>();
        services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
        services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
        services.AddScoped<ITechnicianRepository, TechnicianRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IOtpVerificationRepository, OtpVerificationRepository>();
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

    private static string ResolveConnectionStringForCurrentEnvironment(string connectionString, IConfiguration configuration)
    {
        if (!OperatingSystem.IsLinux() || !IsLocalDbConnectionString(connectionString))
        {
            return connectionString;
        }

        var explicitConnectionString =
            Environment.GetEnvironmentVariable("COOLZO_SQL_CONNECTION")
            ?? configuration["Database:LinuxSqlConnection"];
        if (!string.IsNullOrWhiteSpace(explicitConnectionString))
        {
            return explicitConnectionString;
        }

        var sqlHost =
            Environment.GetEnvironmentVariable("COOLZO_SQL_HOST")
            ?? configuration["Database:LinuxSqlHost"];
        var sqlPort =
            Environment.GetEnvironmentVariable("COOLZO_SQL_PORT")
            ?? configuration["Database:LinuxSqlPort"];
        var sqlUser =
            Environment.GetEnvironmentVariable("COOLZO_SQL_USER")
            ?? configuration["Database:LinuxSqlUser"];
        var sqlPassword =
            Environment.GetEnvironmentVariable("COOLZO_SQL_PASSWORD")
            ?? configuration["Database:LinuxSqlPassword"];

        if (string.IsNullOrWhiteSpace(sqlHost)
            || string.IsNullOrWhiteSpace(sqlUser)
            || string.IsNullOrWhiteSpace(sqlPassword))
        {
            return connectionString;
        }

        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            DataSource = string.IsNullOrWhiteSpace(sqlPort) ? sqlHost : $"{sqlHost},{sqlPort}",
            IntegratedSecurity = false,
            UserID = sqlUser,
            Password = sqlPassword,
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }

    private static bool IsLocalDbConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        return connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)
            || connectionString.Contains("MSSQLLocalDB", StringComparison.OrdinalIgnoreCase);
    }
}
