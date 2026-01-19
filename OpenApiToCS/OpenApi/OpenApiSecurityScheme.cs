using System.Text.Json.Serialization;

namespace OpenApiToCS.OpenApi;

public class OpenApiSecurityScheme
{
    [JsonPropertyName("type")]
    public required string Type { get; init; } // e.g., "apiKey", "http", "oauth2", "openIdConnect"
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    [JsonPropertyName("name")]
    public string? Name { get; init; } // Header/query/cookie name for apiKey
    [JsonPropertyName("in")]
    public string? In { get; init; } // "header", "query", "cookie" for apiKey
    [JsonPropertyName("scheme")]
    public string? Scheme { get; init; } // e.g., "bearer", "basic" for http type
    [JsonPropertyName("bearerFormat")]
    public string? BearerFormat { get; init; } // e.g., "JWT"
    [JsonPropertyName("flows")]
    public OpenApiOAuthFlows? Flows { get; init; }
    [JsonPropertyName("openIdConnectUrl")]
    public string? OpenIdConnectUrl { get; init; }
}

public class OpenApiOAuthFlows
{
    [JsonPropertyName("implicit")]
    public OpenApiOAuthFlow? Implicit { get; init; }
    [JsonPropertyName("password")]
    public OpenApiOAuthFlow? Password { get; init; }
    [JsonPropertyName("clientCredentials")]
    public OpenApiOAuthFlow? ClientCredentials { get; init; }
    [JsonPropertyName("authorizationCode")]
    public OpenApiOAuthFlow? AuthorizationCode { get; init; }
}

public class OpenApiOAuthFlow
{
    [JsonPropertyName("authorizationUrl")]
    public string? AuthorizationUrl { get; init; }
    [JsonPropertyName("tokenUrl")]
    public string? TokenUrl { get; init; }
    [JsonPropertyName("refreshUrl")]
    public string? RefreshUrl { get; init; }
    [JsonPropertyName("scopes")]
    public Dictionary<string, string>? Scopes { get; init; }
}