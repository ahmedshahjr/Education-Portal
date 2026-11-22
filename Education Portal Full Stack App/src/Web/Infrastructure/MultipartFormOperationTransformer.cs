using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EducationPortal.Web.Infrastructure;

/// <summary>
/// A form that can carry files is documented as multipart only, with the same field names the old endpoints gave it
/// (camelCase) and the optional fields nullable. ASP.NET Core lists URL-encoded forms too, and NSwag then generates a
/// URL-encoded body, which cannot carry files; and it makes a non-nullable field required, which breaks an optional one.
/// </summary>
internal sealed class MultipartFormOperationTransformer : IOpenApiOperationTransformer
{
    private const string Multipart = "multipart/form-data";
    private const string UrlEncoded = "application/x-www-form-urlencoded";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation.RequestBody?.Content is not { } content || !content.ContainsKey(Multipart))
        {
            return Task.CompletedTask;
        }

        content.Remove(UrlEncoded);

        if (content[Multipart].Schema is OpenApiSchema schema && schema.Properties is { Count: > 0 } && FormType(context) is { } formType)
        {
            schema.Properties = Rename(schema.Properties, formType);
        }

        return Task.CompletedTask;
    }

    /// <summary>The class bound with [FromForm] in this action, or null.</summary>
    private static Type? FormType(OpenApiOperationTransformerContext context) =>
        (context.Description.ActionDescriptor as ControllerActionDescriptor)?.Parameters
            .OfType<ControllerParameterDescriptor>()
            .FirstOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Form)
            ?.ParameterType;

    private static IDictionary<string, IOpenApiSchema> Rename(IDictionary<string, IOpenApiSchema> properties, Type formType)
    {
        var result = new Dictionary<string, IOpenApiSchema>();
        foreach (var (name, schema) in properties)
        {
            var property = formType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var json = JsonNamingPolicy.CamelCase.ConvertName(name);
            if (property is null)
            {
                result[json] = schema;
                continue;
            }

            var nullable = property.PropertyType == typeof(IFormFile)
                || Nullable.GetUnderlyingType(property.PropertyType) is not null
                || (!property.PropertyType.IsValueType && new NullabilityInfoContext().Create(property).WriteState == NullabilityState.Nullable);

            if (property.PropertyType == typeof(IFormFile))
            {
                result[json] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String | (nullable ? JsonSchemaType.Null : 0),
                    Format = "binary",
                };
                continue;
            }

            if (schema is OpenApiSchema typed && nullable && typed.Type is { } type)
            {
                typed.Type = type | JsonSchemaType.Null;
            }

            result[json] = schema;
        }

        return result;
    }
}
