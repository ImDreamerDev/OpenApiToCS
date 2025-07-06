using System.Text.Json.Serialization;

namespace OpenApiToCS.OpenApi;

public class OpenApiResponse
{
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    [JsonPropertyName("content")]
    public Dictionary<string, OpenApiSchemaContainer>? Content { get; init; } = [];

}