using System.Text.Json.Serialization;

namespace OpenApiToCS.OpenApi;

public class OpenApiPath
{
    [JsonPropertyName("get")]
    public OpenApiOperation? Get { get; init; }
    [JsonPropertyName("post")]
    public OpenApiOperation? Post { get; init; }
    [JsonPropertyName("put")]
    public OpenApiOperation? Put { get; init; }
    [JsonPropertyName("delete")]
    public OpenApiOperation? Delete { get; init; }
    [JsonPropertyName("patch")]
    public OpenApiOperation? Patch { get; init; }
    [JsonPropertyName("head")]
    public OpenApiOperation? Head { get; init; }
    [JsonPropertyName("options")]
    public OpenApiOperation? Options { get; init; }
    [JsonPropertyName("trace")]
    public OpenApiOperation? Trace { get; init; }
}