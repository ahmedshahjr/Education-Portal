namespace EducationPortal.Application.FunctionalTests.Infrastructure;

/// <summary>Small files with real image signatures, and files that only pretend to be images.</summary>
internal static class TestImages
{
    // A 1x1 transparent PNG.
    public static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=");

    // An ICO header with one entry, followed by the PNG above.
    public static readonly byte[] Icon = [0x00, 0x00, 0x01, 0x00, 0x01, 0x00, .. new byte[16], .. Png];

    public static readonly byte[] Svg = "<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>"u8.ToArray();

    public static byte[] OversizedPng(int size) => [.. Png, .. new byte[size - Png.Length]];
}
