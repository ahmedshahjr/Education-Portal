using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EducationPortal.Web.Infrastructure;

/// <summary>
/// Gives each controller action what its minimal API endpoint gave it (ADR-025): the method name as the operation id, and the
/// controller name as the tag, so the generated Angular client keeps its method and class names. The JSON-only media types
/// that MVC adds next to <c>application/json</c> are dropped, so the document matches the endpoints it replaces.
/// </summary>
public sealed class ControllerOperationIdTransformer : IOpenApiOperationTransformer
{
    private static readonly string[] ExtraJsonMediaTypes = ["text/json", "application/*+json"];
    private static readonly string[] ExtraResponseMediaTypes = ["text/plain", "text/json"];

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor action)
        {
            return Task.CompletedTask;
        }

        operation.OperationId = action.MethodInfo.Name;
        operation.Tags = new HashSet<OpenApiTagReference> { new(action.ControllerName) };

        if (operation.RequestBody?.Content is { } requestContent)
        {
            foreach (var mediaType in ExtraJsonMediaTypes)
            {
                requestContent.Remove(mediaType);
            }
        }

        foreach (var response in operation.Responses?.Values ?? Enumerable.Empty<IOpenApiResponse>())
        {
            if (response.Content is { } responseContent)
            {
                foreach (var mediaType in ExtraResponseMediaTypes)
                {
                    responseContent.Remove(mediaType);
                }
            }
        }

        return Task.CompletedTask;
    }
}
