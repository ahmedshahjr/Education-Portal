using System.Globalization;
using System.Reflection;
using FluentValidation.Resources;

namespace EducationPortal.Application.Common.Localization;

/// <summary>
/// Puts validation errors in the request's language (ADR-005). The request localization middleware sets
/// <see cref="CultureInfo.CurrentUICulture"/> before a handler runs, and FluentValidation reads it when it builds a
/// message:
/// <list type="bullet">
/// <item>Our own messages come from <see cref="ValidationMessages"/>, read lazily with <c>WithMessage(_ =&gt; ...)</c>,
/// so each request gets its own language (a plain string would be fixed when the validator is built).</item>
/// <item>Property names in FluentValidation's built-in messages come from <see cref="PropertyNames"/>; a property
/// without an entry keeps FluentValidation's default name.</item>
/// <item>FluentValidation ships Arabic but not Urdu, so <see cref="PortalLanguageManager"/> adds Urdu.</item>
/// </list>
/// </summary>
public static class ValidationLocalization
{
    /// <summary>Sets FluentValidation's global options. Safe to call more than once.</summary>
    public static void Configure()
    {
        ValidatorOptions.Global.LanguageManager = new PortalLanguageManager();
        ValidatorOptions.Global.DisplayNameResolver = ResolveDisplayName;
    }

    /// <summary>The translated name of a validated property, or null for FluentValidation's default.</summary>
    public static string? ResolveDisplayName(Type type, MemberInfo? member, System.Linq.Expressions.LambdaExpression? expression) =>
        member is null ? null : PropertyNames.ResourceManager.GetString(member.Name, CultureInfo.CurrentUICulture);
}

/// <summary>FluentValidation's built-in messages, plus the languages of the portal that it does not ship (Urdu).</summary>
public sealed class PortalLanguageManager : LanguageManager
{
    public PortalLanguageManager()
    {
        foreach (var (key, message) in UrduValidatorMessages.All)
        {
            AddTranslation(Domain.Localization.Language.Urdu.Code, key, message);
        }
    }
}

/// <summary>
/// Urdu for FluentValidation's built-in messages, keyed like its own language files. The placeholders in braces are
/// filled in by FluentValidation and must match the English message (a unit test checks this).
/// </summary>
public static class UrduValidatorMessages
{
    public static IReadOnlyDictionary<string, string> All { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["EmailValidator"] = "'{PropertyName}' درست ای میل پتہ نہیں ہے۔",
        ["GreaterThanOrEqualValidator"] = "'{PropertyName}' کو '{ComparisonValue}' کے برابر یا اس سے بڑا ہونا چاہیے۔",
        ["GreaterThanValidator"] = "'{PropertyName}' کو '{ComparisonValue}' سے بڑا ہونا چاہیے۔",
        ["LengthValidator"] = "'{PropertyName}' کی لمبائی {MinLength} اور {MaxLength} حروف کے درمیان ہونی چاہیے۔ آپ نے {TotalLength} حروف درج کیے۔",
        ["MinimumLengthValidator"] = "'{PropertyName}' کی لمبائی کم از کم {MinLength} حروف ہونی چاہیے۔ آپ نے {TotalLength} حروف درج کیے۔",
        ["MaximumLengthValidator"] = "'{PropertyName}' کی لمبائی زیادہ سے زیادہ {MaxLength} حروف ہو سکتی ہے۔ آپ نے {TotalLength} حروف درج کیے۔",
        ["LessThanOrEqualValidator"] = "'{PropertyName}' کو '{ComparisonValue}' کے برابر یا اس سے چھوٹا ہونا چاہیے۔",
        ["LessThanValidator"] = "'{PropertyName}' کو '{ComparisonValue}' سے چھوٹا ہونا چاہیے۔",
        ["NotEmptyValidator"] = "'{PropertyName}' خالی نہیں ہونا چاہیے۔",
        ["NotEqualValidator"] = "'{PropertyName}' کو '{ComparisonValue}' کے برابر نہیں ہونا چاہیے۔",
        ["NotNullValidator"] = "'{PropertyName}' خالی نہیں ہونا چاہیے۔",
        ["PredicateValidator"] = "'{PropertyName}' کے لیے مقررہ شرط پوری نہیں ہوئی۔",
        ["AsyncPredicateValidator"] = "'{PropertyName}' کے لیے مقررہ شرط پوری نہیں ہوئی۔",
        ["RegularExpressionValidator"] = "'{PropertyName}' درست شکل میں نہیں ہے۔",
        ["EqualValidator"] = "'{PropertyName}' کو '{ComparisonValue}' کے برابر ہونا چاہیے۔",
        ["ExactLengthValidator"] = "'{PropertyName}' کی لمبائی ٹھیک {MaxLength} حروف ہونی چاہیے۔ آپ نے {TotalLength} حروف درج کیے۔",
        ["InclusiveBetweenValidator"] = "'{PropertyName}' کو {From} اور {To} کے درمیان ہونا چاہیے۔ آپ نے {PropertyValue} درج کیا۔",
        ["ExclusiveBetweenValidator"] = "'{PropertyName}' کو {From} اور {To} کے درمیان ہونا چاہیے، ان دونوں کے علاوہ۔ آپ نے {PropertyValue} درج کیا۔",
        ["CreditCardValidator"] = "'{PropertyName}' درست کریڈٹ کارڈ نمبر نہیں ہے۔",
        ["ScalePrecisionValidator"] = "'{PropertyName}' میں کل {ExpectedPrecision} سے زیادہ ہندسے نہیں ہونے چاہئیں، جن میں {ExpectedScale} اعشاریہ ہندسے شامل ہیں۔ {Digits} ہندسے اور {ActualScale} اعشاریہ ہندسے ملے۔",
        ["EmptyValidator"] = "'{PropertyName}' خالی ہونا چاہیے۔",
        ["NullValidator"] = "'{PropertyName}' خالی ہونا چاہیے۔",
        ["EnumValidator"] = "'{PropertyName}' کی اجازت شدہ قدروں میں '{PropertyValue}' شامل نہیں ہے۔",
        ["Length_Simple"] = "'{PropertyName}' کی لمبائی {MinLength} اور {MaxLength} حروف کے درمیان ہونی چاہیے۔",
        ["MinimumLength_Simple"] = "'{PropertyName}' کی لمبائی کم از کم {MinLength} حروف ہونی چاہیے۔",
        ["MaximumLength_Simple"] = "'{PropertyName}' کی لمبائی زیادہ سے زیادہ {MaxLength} حروف ہو سکتی ہے۔",
        ["ExactLength_Simple"] = "'{PropertyName}' کی لمبائی ٹھیک {MaxLength} حروف ہونی چاہیے۔",
        ["InclusiveBetween_Simple"] = "'{PropertyName}' کو {From} اور {To} کے درمیان ہونا چاہیے۔",
    };
}
