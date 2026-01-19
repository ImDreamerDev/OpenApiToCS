using System.Net;
using System.Text.Json.Serialization;

namespace OpenApiToCS.OpenApi;

public class OpenApiOperation
{
    [JsonPropertyName("operationId")]
    public string? OperationId { get; init; }
    [JsonPropertyName("summary")]
    public string? Summary { get; init; }
    [JsonPropertyName("parameters")]
    public OpenApiParameter[]? Parameters { get; init; } = [];
    [JsonPropertyName("requestBody")]
    public OpenApiRequestBody? RequestBody { get; init; }
    [JsonPropertyName("responses")]
    public Dictionary<HttpStatusCode, OpenApiResponse> Responses { get; init; } = [];
}