namespace EducationPortal.Domain.Common;

/// <summary>
/// Parses an enum from its member name only (ENG-ENUM-01). <see cref="Enum.TryParse{TEnum}(string?, bool, out TEnum)"/>
/// also accepts numbers such as <c>"2"</c> or <c>"99"</c>, which would let a caller send a value by number or one that
/// does not exist; this does not.
/// </summary>
public static class EnumNames
{
    public static bool TryParse<TEnum>(string? value, out TEnum result)
        where TEnum : struct, Enum
    {
        var name = value?.Trim();

        if (!string.IsNullOrEmpty(name)
            && !char.IsAsciiDigit(name[0]) && name[0] is not ('-' or '+')
            && Enum.TryParse(name, ignoreCase: true, out result)
            && Enum.IsDefined(result))
        {
            return true;
        }

        result = default;
        return false;
    }
}
