namespace EducationPortal.Application.Modules.Common;

/// <summary>The fields of a module that the master tenant sets when adding or changing it.</summary>
public abstract record ModuleFields
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    /// <summary>True when the module is part of every subscription and never sold on its own.</summary>
    public bool IsCore { get; init; }

    /// <summary>Where the module appears in lists and menus, lowest first.</summary>
    public int SortOrder { get; init; }
}

/// <summary>The rules every module's fields follow (SEC-INPUT-01).</summary>
public abstract class ModuleFieldsValidator<T> : AbstractValidator<T>
    where T : ModuleFields
{
    protected ModuleFieldsValidator()
    {
        RuleFor(m => m.Name).NotEmpty().MaximumLength(PortalModule.NameMaxLength);
        RuleFor(m => m.Description).NotNull().MaximumLength(PortalModule.DescriptionMaxLength);
        RuleFor(m => m.SortOrder).GreaterThanOrEqualTo(0);
    }
}
