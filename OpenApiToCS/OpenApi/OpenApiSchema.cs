using System.Text.Json.Serialization;

namespace OpenApiToCS.OpenApi;

public class OpenApiSchema
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }
    [JsonPropertyName("description")]
    public string? Description { get; init; } // Optional description of the schema
    [JsonPropertyName("example")]
    public object? Example { get; init; } // Optional example value for the schema
    [JsonPropertyName("nullable")]
    public bool Nullable { get; init; } // Indicates if the schema can be null
    [JsonPropertyName("default")]
    public object? Default { get; init; } // Default value for the schema, if applicable
    [JsonPropertyName("deprecated")]
    public bool Deprecated { get; init; } // Indicates if the schema is deprecated
    [JsonPropertyName("required")]
    public List<string>? Required { get; init; } = []; // List of required properties for object schemas
    [JsonPropertyName("type")]
    public string? Type { get; init; } // e.g., "string", "integer", "boolean"
    [JsonPropertyName("format")]
    public string? Format { get; init; } // e.g., "date-time", "int32"
    [JsonPropertyName("$ref")]
    public string? Reference { get; init; } // e.g., "#/components/schemas/SomeModel"
    [JsonPropertyName("properties")]
    public Dictionary<string, OpenApiSchema>? Properties { get; init; } = []; // For complex types, this contains the properties of the object
    [JsonPropertyName("enum")]
    public List<object>? Enum { get; init; } // For string enums, this contains the possible values
    [JsonPropertyName("items")]
    public OpenApiSchema? Items { get; init; }
    [JsonPropertyName("oneOf")]
    public OpenApiSchema[]? OneOf { get; init; }
    [JsonPropertyName("allOf")]
    public OpenApiSchema[]? AllOf { get; init; }

}