using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Plans.Common;

/// <summary>The fields of a plan that the master tenant sets when adding or changing it.</summary>
public abstract record PlanFields
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    /// <summary>The codes of the paid modules in the plan. Core modules are always included and are not listed.</summary>
    public IReadOnlyList<string> Modules { get; init; } = [];
}

/// <summary>The rules every plan's fields follow (SEC-INPUT-01).</summary>
public abstract class PlanFieldsValidator<T> : AbstractValidator<T>
    where T : PlanFields
{
    protected PlanFieldsValidator(IApplicationDbContext context)
    {
        RuleFor(p => p.Name).NotEmpty().MaximumLength(Plan.NameMaxLength);
        RuleFor(p => p.Description).NotNull().MaximumLength(Plan.DescriptionMaxLength);

        RuleFor(p => p.Modules)
            .NotNull()
            .Must(modules => modules.Count <= PortalModule.CodeMaxLength * 10)
            .Must(modules => modules.Distinct(StringComparer.Ordinal).Count() == modules.Count)
            .WithMessage(_ => ValidationMessages.ModuleListedTwice)
            .CustomAsync(async (codes, validation, cancellationToken) =>
            {
                if (codes is null || codes.Count == 0)
                {
                    return;
                }

                var modules = await context.Modules
                    .AsNoTracking()
                    .Where(m => codes.Contains(m.Code))
                    .Select(m => new { m.Code, m.IsCore })
                    .ToListAsync(cancellationToken);

                if (modules.Count != codes.Distinct(StringComparer.Ordinal).Count())
                {
                    validation.AddFailure(ValidationMessages.UnknownModules);
                }
                else if (modules.Any(m => m.IsCore))
                {
                    validation.AddFailure(ValidationMessages.CoreModuleNotSold);
                }
            });
    }
}

internal static class PlanModuleSync
{
    /// <summary>Makes the plan's modules exactly the given codes: adds the missing ones, removes the rest.</summary>
    public static async Task SetModulesAsync(IApplicationDbContext context, Plan plan, IReadOnlyList<string> codes, CancellationToken cancellationToken)
    {
        var wanted = await context.Modules.Where(m => codes.Contains(m.Code)).ToListAsync(cancellationToken);
        var current = await context.PlanModules.Where(m => m.PlanId == plan.Id).ToListAsync(cancellationToken);

        context.PlanModules.RemoveRange(current.Where(c => wanted.All(w => w.Id != c.ModuleId)));
        context.PlanModules.AddRange(wanted.Where(w => current.All(c => c.ModuleId != w.Id)).Select(w => PlanModule.Create(plan, w)));
    }
}

internal static class PlanReader
{
    /// <summary>Live plans in name order, optionally only one, with their live modules in catalogue order.</summary>
    public static async Task<List<PlanDto>> LoadAsync(IApplicationDbContext context, Guid? id, CancellationToken cancellationToken)
    {
        var plans = await context.Plans
            .AsNoTracking()
            .Where(p => id == null || p.PublicId == id)
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.PublicId, p.Code, p.Name, p.Description, p.IsActive })
            .ToListAsync(cancellationToken);

        var planIds = plans.Select(p => p.Id).ToList();
        var modules = await (
                from pm in context.PlanModules.AsNoTracking()
                join m in context.Modules on pm.ModuleId equals m.Id
                where planIds.Contains(pm.PlanId)
                orderby m.SortOrder, m.Code
                select new { pm.PlanId, m.Code })
            .ToListAsync(cancellationToken);
        var byPlan = modules.ToLookup(m => m.PlanId, m => m.Code);

        return plans
            .Select(p => new PlanDto(p.PublicId, p.Code, p.Name, p.Description, p.IsActive, byPlan[p.Id].ToList()))
            .ToList();
    }
}
