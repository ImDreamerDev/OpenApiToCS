using System.Text.Json.Serialization;

namespace OpenApiToCS.OpenApi;

public class OpenApiSchemaContainer
{
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    [JsonPropertyName("schema")]
    public OpenApiSchema Schema { get; init; } = new OpenApiSchema();
}