using System.Text;
using OpenApiToCS.OpenApi;

namespace OpenApiToCS.Generator;

public class BaseGenerator(OpenApiDocument document)
{
    // Enable this to emit metadata comments in the generated code.
    public bool EmitMetadata = false;
    protected readonly OpenApiDocument Document = document;

    protected StringBuilder GenerateSummary(StringBuilder sb, string? summary)
    {
        if (string.IsNullOrEmpty(summary))
            return sb;

        sb.Append("\t/// <summary>\n");
        sb.Append("\t/// ");
        if (summary.Contains('\n'))
        {
            sb.Append(string.Create(summary.Length,
                summary,
                (span, src) =>
                {
                    for (var i = 0; i < src.Length; i++)
                        span[i] = src[i] == '\n' ? ' ' : src[i];
                }));
        }
        else
        {
            sb.Append(summary);
        }

        sb.Append('\n');
        sb.Append("\t/// </summary>\n");
        return sb;
    }

    protected string GenerateSummaryString(string? summary)
    {
        if (string.IsNullOrEmpty(summary))
            return string.Empty;

        StringBuilder sb = new StringBuilder();
        return GenerateSummary(sb, summary).ToString();
    }

    private readonly Dictionary<string, string> _classNamesCache = new Dictionary<string, string>(comparer: StringComparer.Ordinal);


    internal string GetClassNameFromKey(string? key)
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

        // Sanitize: replace invalid C# identifier chars with '_'
        Span<char> buffer = stackalloc char[span.Length];
        var j = 0;
        for (int i = 0; i < span.Length; i++)
        {
            char c = span[i];
            // Allow letters, digits, and underscore only
            // First char must be letter or underscore
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                buffer[j++] = c;
            }
            else
            {
                buffer[j++] = '_';
            }
        }

        var result = buffer[..j].ToString();

        // Ensure it starts with a letter or underscore
        if (result.Length > 0 && char.IsDigit(result[0]))
        {
            result = "_" + result;
        }

        // Ensure it's not empty
        if (string.IsNullOrEmpty(result))
        {
            result = "GeneratedClass";
        }

        _classNamesCache.TryAdd(key, result);
        return result;
    }

    protected string GetTypeFromKey(OpenApiSchema schema, string owningType = "")
    {
        if (schema.Reference is not null)
            return GetClassNameFromKey(schema.Reference);

        string? type = schema.GetPrimaryType();
        string? format = schema.Format;

        return type switch
        {
            null or "object" when schema.Reference is null => "object",
            "integer" when format is null or "int32" => "int",
            "integer" when format == "int64" => "long",
            "number" when format is null or "float" => "float",
            "number" when format == "double" => "double",
            "number" when format == "decimal" => "decimal",
            "boolean" => "bool",
            "string" when format == "date-time" => "DateTimeOffset",
            "string" when format == "date" => "DateOnly",
            "string" when format == "time" => "TimeOnly",
            "string" when format == "uuid" => "Guid",
            "string" when format == "binary" => "byte[]",
            "string" when format == "uri" => "Uri",
            "string" when format is null or "string" => "string",
            "string" when format == "email" => "string",
            "array" => GetArrayType(owningType, schema.Items),
            _ => throw new NotImplementedException($"The schema type {type} with the format {format} is not implemented.")
        };
    }

    private string GetArrayType(string owningType, OpenApiSchema? schema)
    {
        if (schema is null)
            return "object[]";

        if (schema.Reference is not null)
        {
            return GetTypeFromKey(schema) + "[]";
        }

        var primaryType = schema.GetPrimaryType();
        if (primaryType is not null && primaryType is not "object")
        {
            return GetTypeFromKey(schema) + "[]";
        }

        if (owningType == "")
        {
            return "object[]";
        }

        string type = owningType.ToTitleCase();
        return type + "[]";
    }

    protected static bool IsReferenceType(OpenApiSchema schema)
    {
        if (schema.Reference is not null)
            return true;

        var type = schema.GetPrimaryType();
        var format = schema.Format;

        if (schema.IsNullable())
            return true;

        if (schema.Enum is not null)
            return true;

        return type switch
        {
            null or "object" when schema.Reference is null => true,
            "integer" => false,
            "number" => false,
            "boolean" => false,
            "string" when format == "date-time" => false,
            "string" when format == "date" => false,
            "string" when format == "time" => false,
            "string" when format == "uuid" => false,
            "string" => true,
            "array" => true,
            _ => throw new NotImplementedException($"The schema type {type} is not implemented.")
        };
    }

    protected StringBuilder GenerateMetadata(StringBuilder sb, string key, OpenApiSchema schema, int indent = 0)
    {
        if (EmitMetadata is false)
            return sb;

        var indentation = new string(' ', indent * 4);


        sb.AppendLine($"{indentation}// Schema: {key}");
        sb.AppendLine($"{indentation}// Type: {schema.GetPrimaryType()}");
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

        var primaryType = schema.GetPrimaryType();
        if (schema is { Items: not null } && primaryType == "array")
        {
            sb.AppendLine($"{indentation}// Items type: {GetTypeFromKey(schema.Items)}");
        }

        if (schema is { Reference: not null } && (primaryType is null or "object"))
        {
            sb.AppendLine($"{indentation}// Reference: {schema.Reference}");
        }

        if (schema.Default != null)
        {
            sb.AppendLine($"{indentation}// Default value: {schema.Default}");
        }

        sb.AppendLine($"{indentation}// Nullable: " + schema.IsNullable());
        sb.AppendLine($"{indentation}// Deprecated: " + schema.Deprecated);

        if (schema.Required?.Count > 0)
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

        if (schema.Properties?.Count > 0)
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

    protected StringBuilder GenerateMetadata(StringBuilder sb, string key, OpenApiOperation operation, int indent = 0)
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

        if (operation.Parameters?.Length > 0)
        {
            sb.AppendLine($"{indentation}// Parameters:");
            foreach (OpenApiParameter parameter in operation.Parameters)
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
                if (response.Value.Content?.Count > 0)
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

    protected OpenApiSchema? GetSchemaFromReference(string reference)
    {
        if (string.IsNullOrEmpty(reference))
            return null;

        string className = reference.Replace("#/components/schemas/", "");
        return Document.Components?.Schemas.GetValueOrDefault(className);
    }
}