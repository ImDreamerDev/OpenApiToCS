using System.Text;
using OpenApiToCS.OpenApi;

namespace OpenApiToCS.Generator;

public class BaseGenerator
{
    public static bool EmitMetadata = false;

    protected static StringBuilder GenerateSummary(StringBuilder sb, string? summary)
    {
        if (string.IsNullOrEmpty(summary))
            return sb;

        sb.AppendLine($"\t/// <summary>");
        sb.AppendLine($"\t/// {summary.Replace("\n", " ")}");
        sb.AppendLine($"\t/// </summary>");

        return sb;
    }

    private static readonly Dictionary<string, string> _classNamesCache = new Dictionary<string, string>(comparer: StringComparer.Ordinal);


    protected static string GetClassNameFromKey(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return "object";

        if (_classNamesCache.TryGetValue(key, out string? cachedClassName))
            return cachedClassName;

        var span = key.AsSpan();
        int lastSlash = span.LastIndexOf('/');
        if (lastSlash != -1)
            span = span[(lastSlash + 1)..];

        int lastDot = span.LastIndexOf('.');
        if (lastDot != -1)
            span = span[(lastDot + 1)..];

        // Sanitize: replace invalid chars with '_'
        Span<char> buffer = stackalloc char[span.Length];
        var j = 0;
        foreach (char c in span)
        {
            buffer[j++] = c is '-' or '.' or '{' or '}' or ' ' ? '_' : c;
        }
        var result = buffer[..j].ToString();

        _classNamesCache.TryAdd(key, result);
        return result;
    }

    protected static string GetTypeFromKey(OpenApiSchema schema)
    {
        if (schema.Reference is not null)
            return GetClassNameFromKey(schema.Reference);

        string? type = schema.Type;
        string? format = schema.Format;

        return type switch
        {
            null or "object" when schema.Reference is null => "object",
            "integer" when format is null or "int32" => "int",
            "integer" when format == "int64" => "long",
            "number" when format is null or "float" => "float",
            "number" when format == "double" => "double",
            "boolean" => "bool",
            "string" when format == "date-time" => "DateTimeOffset",
            "string" when format == "date" => "DateOnly",
            "string" when format == "time" => "TimeOnly",
            "string" when format == "uuid" => "Guid",
            "string" when format == "binary" => "byte[]",
            "string" when format == "uri" => "Uri",
            "string" when format is null or "string" => "string",
            "array" => schema.Items != null
                ? $"{GetTypeFromKey(schema.Items)}[]"
                : "object[]",
            _ => throw new NotImplementedException($"The schema type {type} is not implemented.")
        };
    }

    protected static StringBuilder GenerateMetadata(StringBuilder sb, string key, OpenApiSchema schema, int indent = 0)
    {
        if (EmitMetadata is false)
            return sb;

        var indentation = new string(' ', indent * 4);


        sb.AppendLine($"{indentation}// Schema: {key}");
        sb.AppendLine($"{indentation}// Type: {schema.Type}");
        sb.AppendLine($"{indentation}// Format: {schema.Format ?? "N/A"}");
        if (schema.Description is not null)
            sb.AppendLine($"{indentation}// Description: {schema.Description.Replace("\n", " ")}");
        if (schema.Enum is not null)
        {
            sb.AppendLine($"{indentation}// Enum values:");
            foreach (object enumValue in schema.Enum)
            {
                sb.AppendLine($"{indentation}// - {enumValue.ToString()}");
            }
        }

        if (schema.Type == "array" && schema.Items != null)
        {
            sb.AppendLine($"{indentation}// Items type: {GetTypeFromKey(schema.Items)}");
        }

        if (schema is { Type: null or "object", Reference: not null })
        {
            sb.AppendLine($"{indentation}// Reference: {schema.Reference}");
        }

        if (schema.Default != null)
        {
            sb.AppendLine($"{indentation}// Default value: {schema.Default}");
        }

        sb.AppendLine($"{indentation}// Nullable: " + schema.Nullable);
        sb.AppendLine($"{indentation}// Deprecated: " + schema.Deprecated);

        if (schema.Required.Count > 0)
        {
            sb.AppendLine($"{indentation}// Required properties:");
            foreach (string requiredProperty in schema.Required)
            {
                sb.AppendLine($"// - {requiredProperty}");
            }
        }

        if (schema.Format is not null)
        {
            sb.AppendLine($"{indentation}// Format: {schema.Format}");
        }

        if (schema.Properties.Count > 0)
        {
            sb.AppendLine($"{indentation}// Properties:");
            foreach (var property in schema.Properties)
            {
                sb.AppendLine($"{indentation}// - {property.Key}: {GetTypeFromKey(property.Value)}");
                sb = GenerateMetadata(sb, property.Key, property.Value, indent + 1);
            }
        }


        return sb;
    }

    protected static StringBuilder GenerateMetadata(StringBuilder sb, string key, OpenApiOperation operation, int indent = 0)
    {
        if (EmitMetadata is false)
            return sb;

        var indentation = new string(' ', indent * 4);
        sb.AppendLine($"{indentation}// Operation: {key}");
        if (operation.Summary is not null)
        {
            sb.AppendLine($"{indentation}// Summary: {operation.Summary.Replace("\n", " ")}");
        }
        if (operation.RequestBody is not null)
        {
            sb.AppendLine($"{indentation}// Request Body: {operation.RequestBody.Description?.Replace("\n", " ")}");
            foreach (var content in operation.RequestBody.Content)
            {
                sb.AppendLine($"{indentation}// Content Type: {content.Key}");
                sb = GenerateMetadata(sb, content.Key, content.Value.Schema, indent + 1);
            }
        }

        if (operation.Parameters.Length > 0)
        {
            sb.AppendLine($"{indentation}// Parameters:");
            foreach (var parameter in operation.Parameters)
            {
                sb.AppendLine($"{indentation}// - {parameter.Name} ({parameter.In})");
                sb.AppendLine($"{indentation}//   Description: {parameter.Description?.Replace("\n", " ")}");
                sb.AppendLine($"{indentation}//   Required: {parameter.Required}");
                sb.AppendLine($"{indentation}//   Deprecated: {parameter.Deprecated}");
                sb = GenerateMetadata(sb, parameter.Name, parameter.Schema, indent + 1);
            }
        }

        if (operation.Responses.Count > 0)
        {
            sb.AppendLine($"{indentation}// Responses:");
            foreach (var response in operation.Responses)
            {
                sb.AppendLine($"{indentation}// - {response.Key}: {response.Value.Description?.Replace("\n", " ")}");
                if (response.Value.Content.Count > 0)
                {
                    foreach (var content in response.Value.Content)
                    {
                        sb.AppendLine($"{indentation}//   Content Type: {content.Key}");
                        sb = GenerateMetadata(sb, content.Key, content.Value.Schema, indent + 1);
                    }
                }
            }
        }

        return sb;
    }
}