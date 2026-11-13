using System.Reflection;
using EducationPortal.Application.Common.Behaviours;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(cfg => 
            cfg.AddMaps(Assembly.GetExecutingAssembly()));

        builder.Services.AddScoped<EducationPortal.Application.Common.Interfaces.IModuleAccess, EducationPortal.Application.Modules.Common.ModuleAccess>();
        builder.Services.AddSingleton<EducationPortal.Application.Common.Interfaces.IPaymentProvider, EducationPortal.Application.Invoices.Common.ManualPaymentProvider>();

        // The daily subscription lifecycle (ADR-015). Bad settings stop the app at start-up rather than at 01:00.
        builder.Services.AddOptions<EducationPortal.Application.Subscriptions.Lifecycle.SubscriptionLifecycleOptions>()
            .Bind(builder.Configuration.GetSection(EducationPortal.Application.Subscriptions.Lifecycle.SubscriptionLifecycleOptions.SectionName))
            .Validate(
                o => System.ComponentModel.DataAnnotations.Validator.TryValidateObject(o, new System.ComponentModel.DataAnnotations.ValidationContext(o), null, validateAllProperties: true),
                "The SubscriptionLifecycle settings are not valid: GracePeriodDays 0 to 90, ReminderDays distinct and 1 to 365, Schedule set.")
            .ValidateOnStart();
        builder.Services.AddScoped<EducationPortal.Application.Subscriptions.Lifecycle.SubscriptionLifecycle>();

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        EducationPortal.Application.Common.Localization.ValidationLocalization.Configure();

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });
    }
}
