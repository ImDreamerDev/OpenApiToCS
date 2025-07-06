using System.Text.Json.Serialization;

namespace OpenApiToCS.OpenApi;

public class OpenApiParameter
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    [JsonPropertyName("in")]
    public required string In { get; init; } // e.g., "query", "header", "path"
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    [JsonPropertyName("required")]
    public bool Required { get; init; }
    [JsonPropertyName("deprecated")]
    public bool Deprecated { get; init; } // Indicates if the parameter is deprecated
    [JsonPropertyName("schema")]
    public required OpenApiSchema Schema { get; init; }
}