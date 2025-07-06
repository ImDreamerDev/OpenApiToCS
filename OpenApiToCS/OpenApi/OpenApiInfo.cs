using System.Text.Json.Serialization;

namespace OpenApiToCS.OpenApi;

public class OpenApiInfo
{
    [JsonPropertyName("title")]
    public required string Title { get; init; }
    [JsonPropertyName("summary")]
    public string? Summary { get; init; }
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    [JsonPropertyName("version")]
    public required string Version { get; init; }

}