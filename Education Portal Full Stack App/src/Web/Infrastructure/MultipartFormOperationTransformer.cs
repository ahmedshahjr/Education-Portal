using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EducationPortal.Web.Infrastructure;

/// <summary>
/// A form that can carry files is documented as multipart only. ASP.NET Core also lists URL-encoded forms, and NSwag
/// then generates a URL-encoded body, which cannot carry files.
/// </summary>
internal sealed class MultipartFormOperationTransformer : IOpenApiOperationTransformer
{
    private const string Multipart = "multipart/form-data";
    private const string UrlEncoded = "application/x-www-form-urlencoded";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation.RequestBody?.Content is { } content && content.ContainsKey(Multipart))
        {
            content.Remove(UrlEncoded);
        }

        return Task.CompletedTask;
    }
}
