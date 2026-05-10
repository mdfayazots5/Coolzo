using Coolzo.Application.Common.Interfaces;
using Coolzo.Persistence.Context;
using Coolzo.Persistence.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Coolzo.Persistence.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{
    private const string SqlServerProvider = "SqlServer";
    private const string PostgreSqlProvider = "Postgres";
    private const string DefaultLocalDbConnection =
        "Server=(localdb)\\MSSQLLocalDB;Database=Coolzo;Trusted_Connection=True;TrustServerCertificate=True;";

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = ResolveDatabaseProvider(configuration);
        var connectionString = ResolveConnectionString(configuration, provider);

        services.AddDbContext<CoolzoDbContext>(options =>
        {
            if (IsPostgreSqlProvider(provider))
            {
                var npgsqlBuilder = new NpgsqlConnectionStringBuilder(connectionString)
                {
                    Timeout = 10,
                    CommandTimeout = 30,
                };

                options.UseNpgsql(npgsqlBuilder.ConnectionString, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
                    npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
                return;
            }

            options.UseSqlServer(connectionString, sqlServer =>
            {
                sqlServer.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            });
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

    private static string ResolveConnectionString(IConfiguration configuration, string provider)
    {
        if (IsPostgreSqlProvider(provider))
        {
            var postgresConnectionString =
                Environment.GetEnvironmentVariable("COOLZO_POSTGRES_CONNECTION")
                ?? configuration.GetConnectionString("PostgresConnection");

            if (!string.IsNullOrWhiteSpace(postgresConnectionString))
            {
                return postgresConnectionString;
            }

            throw new InvalidOperationException(
                "Database provider 'Postgres' is enabled, but no PostgreSQL connection string was configured. Set ConnectionStrings:PostgresConnection or COOLZO_POSTGRES_CONNECTION.");
        }

        var sqlServerConnectionString =
            Environment.GetEnvironmentVariable("COOLZO_SQL_CONNECTION")
            ?? configuration.GetConnectionString("SqlServerConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? DefaultLocalDbConnection;

        return ResolveSqlServerConnectionStringForCurrentEnvironment(sqlServerConnectionString, configuration);
    }

    private static string ResolveDatabaseProvider(IConfiguration configuration)
    {
        var provider =
            Environment.GetEnvironmentVariable("COOLZO_DB_PROVIDER")
            ?? configuration["Database:Provider"]
            ?? SqlServerProvider;

        if (string.Equals(provider, SqlServerProvider, StringComparison.OrdinalIgnoreCase)
            || string.Equals(provider, PostgreSqlProvider, StringComparison.OrdinalIgnoreCase)
            || string.Equals(provider, "PostgreSql", StringComparison.OrdinalIgnoreCase)
            || string.Equals(provider, "Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            return provider;
        }

        throw new InvalidOperationException(
            $"Unsupported database provider '{provider}'. Supported values: {SqlServerProvider}, {PostgreSqlProvider}.");
    }

    private static string ResolveSqlServerConnectionStringForCurrentEnvironment(string connectionString, IConfiguration configuration)
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

    private static bool IsPostgreSqlProvider(string provider)
    {
        return string.Equals(provider, PostgreSqlProvider, StringComparison.OrdinalIgnoreCase)
            || string.Equals(provider, "PostgreSql", StringComparison.OrdinalIgnoreCase)
            || string.Equals(provider, "Npgsql", StringComparison.OrdinalIgnoreCase);
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
