using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenApiToCS.OpenApi;

public class OpenApiSchema
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    [JsonPropertyName("example")]
    public object? Example { get; init; }
    [JsonPropertyName("examples")]
    public Dictionary<string, OpenApiExample>? Examples { get; init; }
    [JsonPropertyName("nullable")]
    public bool Nullable { get; init; }
    [JsonPropertyName("default")]
    public object? Default { get; init; }
    [JsonPropertyName("deprecated")]
    public bool Deprecated { get; init; }
    [JsonPropertyName("required")]
    public List<string>? Required { get; init; } = [];
    [JsonPropertyName("type")]
    [JsonConverter(typeof(StringOrArrayConverter))]
    public StringOrArray? Type { get; init; }
    [JsonPropertyName("format")]
    public string? Format { get; init; }
    [JsonPropertyName("$ref")]
    public string? Reference { get; init; }
    [JsonPropertyName("properties")]
    public Dictionary<string, OpenApiSchema>? Properties { get; init; } = [];
    [JsonPropertyName("enum")]
    public List<object>? Enum { get; init; }
    [JsonPropertyName("const")]
    public object? Const { get; init; }
    [JsonPropertyName("items")]
    public OpenApiSchema? Items { get; init; }
    [JsonPropertyName("prefixItems")]
    public OpenApiSchema[]? PrefixItems { get; init; }
    [JsonPropertyName("oneOf")]
    public OpenApiSchema[]? OneOf { get; init; }
    [JsonPropertyName("allOf")]
    public OpenApiSchema[]? AllOf { get; init; }
    [JsonPropertyName("anyOf")]
    public OpenApiSchema[]? AnyOf { get; init; }
    [JsonPropertyName("discriminator")]
    public OpenApiDiscriminator? Discriminator { get; init; }
    [JsonPropertyName("minLength")]
    public int? MinLength { get; init; }
    [JsonPropertyName("maxLength")]
    public int? MaxLength { get; init; }
    [JsonPropertyName("minimum")]
    public decimal? Minimum { get; init; }
    [JsonPropertyName("maximum")]
    public decimal? Maximum { get; init; }
    [JsonPropertyName("exclusiveMinimum")]
    [JsonConverter(typeof(ExclusiveMinMaxConverter))]
    public ExclusiveValue? ExclusiveMinimum { get; init; }
    [JsonPropertyName("exclusiveMaximum")]
    [JsonConverter(typeof(ExclusiveMinMaxConverter))]
    public ExclusiveValue? ExclusiveMaximum { get; init; }
    [JsonPropertyName("multipleOf")]
    public decimal? MultipleOf { get; init; }
    
    // Helper method to check if nullable in either 3.0 or 3.1 format
    public bool IsNullable()
    {
        // OpenAPI 3.0: nullable: true
        if (Nullable) return true;
        
        // OpenAPI 3.1: type: ["string", "null"]
        if (Type?.IsArray == true && Type.Values.Contains("null"))
            return true;
            
        return false;
    }
    
    // Helper to get primary type (handling both string and array)
    public string? GetPrimaryType()
    {
        if (Type == null) return null;
        if (Type.IsArray)
        {
            // Return first non-null type
            return Type.Values.FirstOrDefault(t => t != "null");
        }
        return Type.Value;
    }
}

public class OpenApiExample
{
    [JsonPropertyName("value")]
    public object? Value { get; init; }
    [JsonPropertyName("summary")]
    public string? Summary { get; init; }
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

public class OpenApiDiscriminator
{
    [JsonPropertyName("propertyName")]
    public string? PropertyName { get; init; }
    [JsonPropertyName("mapping")]
    public Dictionary<string, string>? Mapping { get; init; }
}