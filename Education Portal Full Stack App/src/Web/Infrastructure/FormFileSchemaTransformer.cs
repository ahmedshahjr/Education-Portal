using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EducationPortal.Web.Infrastructure;

/// <summary>
/// Describes optional <see cref="IFormFile"/> properties of a form as nullable binary strings. The default description, a
/// union of null and a reference, makes NSwag generate a URL-encoded body instead of multipart form data, so the
/// generated Angular client could not upload files.
/// </summary>
internal sealed class FormFileSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema.Properties is null)
        {
            return Task.CompletedTask;
        }

        var fileProperties = context.JsonTypeInfo.Properties
            .Where(property => property.PropertyType == typeof(IFormFile))
            .Select(property => property.Name)
            .Where(schema.Properties.ContainsKey)
            .ToList();

        foreach (var name in fileProperties)
        {
            schema.Properties[name] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Format = "binary" };
        }

        return Task.CompletedTask;
    }
}
