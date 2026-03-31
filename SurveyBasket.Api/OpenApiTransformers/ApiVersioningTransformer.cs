using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace SurveyBasket.Api.OpenApiTransformers;

public sealed class ApiVersioningTransformer(ApiVersionDescription description) : IOpenApiDocumentTransformer
{
    public ApiVersionDescription Description { get; } = description;

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new()
        {
            Title = "Survey Basket API",
            Version = Description.ApiVersion.ToString(),
            Description = $"API Description.{(Description.IsDeprecated ? " This API version has been deprecated." : string.Empty)}",
        };

        return Task.CompletedTask;
    }
}