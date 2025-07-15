using System.Text.Json.Serialization;

namespace OpenApiToCS.OpenApi;

public class OpenApiSecurityScheme
{
    [JsonPropertyName("type")]
    public required string Type { get; init; } // e.g., "apiKey", "http", "oauth2"
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    [JsonPropertyName("scheme")]
    public string? Scheme { get; init; } // e.g., "bearer", "basic"
    [JsonPropertyName("bearerFormat")]
    public string? BearerFormat { get; init; } // e.g., "JWT"
}