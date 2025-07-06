using System.Text;
using OpenApiToCS.OpenApi;

namespace OpenApiToCS.Generator;

public class DataClassGenerator : BaseGenerator
{
    public static Dictionary<string, string> GenerateDataClasses(OpenApiDocument document)
    {
        var result = new Dictionary<string, string>();

        var classNames = new HashSet<string>();
        foreach (var schema in document.Components.Schemas)
        {
            if (schema.Value.Type is not null and not "object" && schema.Value is { Type: not "string", Enum: null })
            {
                Console.Error.WriteLine("Unsupported schema type: " + schema.Value.Type + " for key: " + schema.Key);
                continue;
            }

            string className = GetClassNameFromKey(schema.Key).ToTitleCase();
            string namespaceName = GetClassNameFromKey(document.Info.Title).ToTitleCase() + "ApiClient" + "V" + document.Info.Version[0] + ".Models";
            if (className is "ProblemDetails" or "HttpValidationProblemDetails" or "ExceptionProblemDetails")
                continue;

            if (classNames.Add(className) is false)
            {
                continue;
            }
            StringBuilder sb = new StringBuilder();

            if (schema.Value.Enum is not null)
            {
                sb = GenerateEnum(sb, className, namespaceName, schema.Key, schema.Value);
                result.Add(className, sb.ToString());
                continue;
            }

            sb = GenerateRecord(sb, className, namespaceName, schema.Key, schema.Value);
            result.Add(className, sb.ToString());
        }

        return result;
    }

    private static StringBuilder GenerateEnum(StringBuilder sb, string className, string namespaceName, string key, OpenApiSchema schema)
    {
        if (schema.Enum is null)
        {
            Console.Error.WriteLine("Enum schema does not contain any enum values for key: " + key);
            return sb;
        }

        sb.AppendLine("using System.Text.Json.Serialization;");
        sb.AppendLine($"namespace {namespaceName};");
        sb = GenerateMetadata(sb, key, schema);
        sb = GenerateSummary(sb, schema.Description);
        sb.AppendLine("\t[JsonConverter(typeof(JsonStringEnumConverter))]");
        sb.AppendLine($"\tpublic enum {className}");
        sb.AppendLine("{");
        foreach (object enumValue in schema.Enum)
        {
            sb.AppendLine($"        {enumValue.ToString()},");
        }
        sb.AppendLine("}");
        return sb;
    }

    private static StringBuilder GenerateRecord(StringBuilder sb, string className, string namespaceName, string key, OpenApiSchema schema)
    {
        if (schema.Reference is not null)
        {
            sb.AppendLine($"using {namespaceName};");
        }
        if (schema.Required?.Count != 0)
        {
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
        }

        if (schema.Properties is not null)
        {
            foreach (OpenApiSchema prop in schema.Properties.Values)
            {
                if (prop.Reference is not null)
                {
                    sb.AppendLine($"using {namespaceName};");
                }
            }
        }

        sb.AppendLine("using System.Text.Json.Serialization;");
        sb = GenerateMetadata(sb, key, schema);
        sb.AppendLine("public record " + className);
        sb.AppendLine("{");
        if (schema.Properties is not null)
        {
            foreach (var property in schema.Properties)
            {
                GenerateSummary(sb, property.Value.Description);

                if (property.Value.Deprecated)
                {
                    sb.AppendLine("\t[Obsolete(\"This property is deprecated.\")]");
                }

                if (schema.Required is not null && schema.Required.Contains(property.Key))
                {
                    sb.AppendLine("\t[Required]");
                }

                sb.AppendLine("\t[JsonPropertyName(\"" + property.Key + "\")]");
                if (property.Value.Type is not "object" and not null)
                {
                    sb.AppendLine($"\tpublic {GetTypeFromKey(property.Value)}{(property.Value.Nullable ? "?" : "")} {property.Key.ToTitleCase()} {{ get; set; }}");
                }
                else if (property.Value.Reference is not null)
                {
                    sb.AppendLine($"\tpublic {GetClassNameFromKey(property.Value.Reference)}{(property.Value.Nullable ? "?" : "")} {property.Key.ToTitleCase()} {{ get; set; }}");
                }
                else
                    sb.AppendLine($"\tpublic {GetClassNameFromKey(property.Value.Type)}{(property.Value.Nullable ? "?" : "")} {property.Key.ToTitleCase()} {{ get; set; }}");
                sb.AppendLine();
            }
        }
        

        sb.AppendLine("}");
        return sb;
    }
}