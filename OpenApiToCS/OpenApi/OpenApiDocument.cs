using System.Text.Json.Serialization;

namespace OpenApiToCS.OpenApi;

public class OpenApiDocument
{
    [JsonPropertyName("openapi")]
    public required string OpenApiVersion { get; init; }
    [JsonPropertyName("info")]
    public required OpenApiInfo Info { get; init; }
    [JsonPropertyName("paths")]
    public Dictionary<string, OpenApiPath> Paths { get; init; } = [];
    [JsonPropertyName("webhooks")]
    public Dictionary<string, OpenApiPath>? Webhooks { get; init; } = [];
    [JsonPropertyName("components")]
    public OpenApiComponents? Components { get; init; }
    
    public bool IsOpenApi31 => OpenApiVersion.StartsWith("3.1");
}