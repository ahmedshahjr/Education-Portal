namespace EducationPortal.Application.Branches.Common;

/// <summary>Picks the four-digit code a new campus gets: unique within its school (the main campus already has <c>0001</c>).</summary>
public static class CampusCodes
{
    /// <summary>
    /// A random four-digit code that is not in <paramref name="used"/>, or <c>null</c> when every code is taken. It tries a few
    /// random codes first, then the first free one, so the choice is random but the search always ends.
    /// </summary>
    public static string? NextFree(IReadOnlySet<string> used, Random random)
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            var candidate = random.Next(1, 10000).ToString("D4", System.Globalization.CultureInfo.InvariantCulture);
            if (!used.Contains(candidate))
            {
                return candidate;
            }
        }

        for (var number = 1; number < 10000; number++)
        {
            var candidate = number.ToString("D4", System.Globalization.CultureInfo.InvariantCulture);
            if (!used.Contains(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
