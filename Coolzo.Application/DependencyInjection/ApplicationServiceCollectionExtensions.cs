using Coolzo.Application.Common.Behaviors;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Security;
using Coolzo.Application.Common.Services;
using Coolzo.Application.Features.Billing;
using Coolzo.Application.Features.Amc;
using Coolzo.Application.Features.Inventory;
using Coolzo.Application.Features.Support;
using Coolzo.Application.Features.TechnicianJob;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Coolzo.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<AmcScheduleService>();
        services.AddScoped<IBillingCalculationService, BillingCalculationService>();
        services.AddScoped<AdminActivityLogger>();
        services.AddScoped<GapPhaseAFeatureFlagService>();
        services.AddScoped<GapPhaseANotificationService>();
        services.AddScoped<GapPhaseAValidationService>();
        services.AddScoped<GapPhaseAWorkflowService>();
        services.AddScoped<TechnicianOnboardingEligibilityService>();
        services.AddScoped<HelperAssignmentValidationService>();
        services.AddScoped<InstallationLifecycleWorkflowService>();
        services.AddScoped<InstallationLifecycleAccessService>();
        services.AddScoped<CancellationPolicyEvaluationService>();
        services.AddScoped<RefundApprovalRulesService>();
        services.AddScoped<CustomerAbsentOrchestrationService>();
        services.AddScoped<BillingAccessService>();
        services.AddScoped<IJobCardFactory, JobCardFactory>();
        services.AddScoped<InventoryAccessService>();
        services.AddScoped<InventoryCatalogService>();
        services.AddScoped<InventoryStockService>();
        services.AddScoped<ServiceLifecycleAccessService>();
        services.AddScoped<SupportTicketAccessService>();
        services.AddScoped<TechnicianJobLifecycleResolver>();
        services.AddScoped<ITechnicianFieldExecutionService, TechnicianFieldExecutionService>();
        services.AddScoped<ITechnicianJobAccessService, TechnicianJobAccessService>();
        services.AddScoped<ICustomerPasswordPolicyService, CustomerPasswordPolicyService>();
        services.AddScoped<AuthenticatedUserProfileFactory>();
        services.AddScoped<CustomerAccountLookupService>();
        services.AddScoped<CustomerAccountProvisioningService>();

        return services;
    }
}
