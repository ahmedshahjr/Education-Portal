using System.Diagnostics;

namespace EducationPortal.Application.Common.Diagnostics;

/// <summary>
/// The Application layer's <see cref="System.Diagnostics.ActivitySource"/>. A use case that needs its own span starts
/// one here; see docs/observability.md. Starting a span costs nothing when no listener is attached.
/// </summary>
public static class ApplicationDiagnostics
{
    public static readonly ActivitySource ActivitySource = new(TelemetryNames.Application);
}
