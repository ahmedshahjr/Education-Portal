using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<EducationPortal.Application.Common.Interfaces.IModuleAccess, EducationPortal.Application.Modules.Common.ModuleAccess>();
        builder.Services.AddScoped<EducationPortal.Application.Common.Interfaces.IRoleRights, EducationPortal.Application.SchoolRoles.Common.RoleRights>();
        builder.Services.AddScoped<EducationPortal.Application.Common.Security.IRequestAuthorizer, EducationPortal.Application.Common.Security.RequestAuthorizer>();
        builder.Services.AddScoped<EducationPortal.Application.Common.Security.IRequestRules, EducationPortal.Application.Common.Security.RequestRules>();
        builder.Services.AddScoped<EducationPortal.Application.Common.Interfaces.ICampusAccess, EducationPortal.Application.Campuses.Common.CampusAccess>();
        builder.Services.AddSingleton<EducationPortal.Application.Common.Interfaces.IPaymentProvider, EducationPortal.Application.Invoices.Common.ManualPaymentProvider>();

        // The daily subscription lifecycle (ADR-015). Bad settings stop the app at start-up rather than at 01:00.
        builder.Services.AddOptions<EducationPortal.Application.Subscriptions.Lifecycle.SubscriptionLifecycleOptions>()
            .Bind(builder.Configuration.GetSection(EducationPortal.Application.Subscriptions.Lifecycle.SubscriptionLifecycleOptions.SectionName))
            .Validate(
                o => System.ComponentModel.DataAnnotations.Validator.TryValidateObject(o, new System.ComponentModel.DataAnnotations.ValidationContext(o), null, validateAllProperties: true),
                "The SubscriptionLifecycle settings are not valid: GracePeriodDays 0 to 90, ReminderDays distinct and 1 to 365, Schedule set.")
            .ValidateOnStart();
        builder.Services.AddScoped<EducationPortal.Application.Subscriptions.Lifecycle.SubscriptionLifecycle>();


        // Every use case is a plain service named ...Handler (ADR-025). Each is registered here so a controller can inject it.
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.Name.EndsWith("Handler", StringComparison.Ordinal)))
        {
            builder.Services.AddScoped(type);
        }

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        EducationPortal.Application.Common.Localization.ValidationLocalization.Configure();

    }
}
