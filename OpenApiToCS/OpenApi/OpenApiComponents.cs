using System.Text.Json.Serialization;

namespace OpenApiToCS.OpenApi;

public class OpenApiComponents
{
    [JsonPropertyName("schemas")]
    public Dictionary<string, OpenApiSchema> Schemas { get; init; } = [];
    [JsonPropertyName("securitySchemes")]
    public Dictionary<string, OpenApiSecurityScheme> SecuritySchemes { get; init; } = [];
}