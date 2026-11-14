namespace EducationPortal.Application.Plans.Common;

/// <summary>A plan: a bundle of paid modules. Core modules are in every subscription, so they are not listed.</summary>
/// <param name="Id">The plan's public id.</param>
/// <param name="Modules">The codes of the paid modules in the plan, in catalogue order.</param>
public sealed record PlanDto(Guid Id, string Code, string Name, string Description, bool IsActive, IReadOnlyList<string> Modules);
