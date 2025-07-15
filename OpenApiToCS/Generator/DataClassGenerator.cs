using System.Text;
using OpenApiToCS.OpenApi;

namespace OpenApiToCS.Generator;

public class DataClassGenerator : BaseGenerator
{
    private readonly Queue<(string name, OpenApiSchema schema)> _missingSchemasToGenerate = [];
    private readonly HashSet<string> _generatedSchemas = [];
    private readonly Queue<(string className, string source)> _generatedOneOfs = [];

    public Dictionary<string, string> GenerateDataClasses(OpenApiDocument document)
    {
        var result = new Dictionary<string, string>();
        string namespaceName = GetClassNameFromKey(document.Info.Title).ToTitleCase() + "ApiClient" + "V" + document.Info.Version[0] + ".Models";
        foreach (var schema in document.Components.Schemas)
        {
            if (schema.Value.Type is not null and not "object" && schema.Value is { Type: not "string", Enum: null })
            {
                Console.Error.WriteLine("Unsupported schema type: " + schema.Value.Type + " for key: " + schema.Key);
                continue;
            }

            string className = GetClassNameFromKey(schema.Key).ToTitleCase();
            if (className is "ProblemDetails" or "HttpValidationProblemDetails" or "ExceptionProblemDetails")
                continue;

            if (_generatedSchemas.Add(className) is false)
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

        while (_missingSchemasToGenerate.TryDequeue(out (string name, OpenApiSchema schema) tuple))
        {
            string className = GetClassNameFromKey(tuple.name).ToTitleCase();
            if (_generatedSchemas.Add(className) is false)
            {
                continue; // Already generated
            }

            StringBuilder sb = new StringBuilder();
            sb = GenerateRecord(sb, className, namespaceName, tuple.name, tuple.schema);
            result.Add(className, sb.ToString());
        }

        while (_generatedOneOfs.TryDequeue(out (string className, string source) tuple))
        {
            result.Add(tuple.className, tuple.source);
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

    private StringBuilder GenerateRecord(StringBuilder sb, string className, string namespaceName, string key, OpenApiSchema schema, string? baseClass = null)
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
        sb.AppendLine($"namespace {namespaceName};");
        sb = GenerateMetadata(sb, key, schema);
        sb.Append("public record " + className);
        if (baseClass is not null)
        {
            sb.Append(" : " + baseClass);
        }
        sb.AppendLine();
        sb.AppendLine("{");
        if (schema.Properties is not null)
        {
            foreach (var property in schema.Properties)
            {
                sb = GenerateProperty(sb, property, schema, className);

                if (property.Value.Items?.OneOf != null)
                {
                    GenerateOneOf(property.Key, namespaceName, property.Value.Items);
                }
            }
        }

        sb.AppendLine("}");
        return sb;
    }

    private StringBuilder GenerateProperty(StringBuilder sb, KeyValuePair<string, OpenApiSchema> property, OpenApiSchema schema, string className)
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

        string propertyName = property.Key.ToTitleCase();
        string? propertyType;

        sb.AppendLine("\t[JsonPropertyName(\"" + property.Key + "\")]");
        if (property.Value.Type is not "object" and not null)
        {
            if (property.Value.Items?.OneOf is not null)
            {
                propertyType = property.Key.ToTitleCase() + "OneOf[]";
            }
            else
                propertyType = GetTypeFromKey(property.Value);
        }
        else if (property.Value.Reference is not null)
        {
            propertyType = GetClassNameFromKey(property.Value.Reference);
        }
        else if (property.Value.Type is null)
        {
            propertyType = "object";
        }
        else
        {
            _missingSchemasToGenerate.Enqueue((property.Key, property.Value));
            propertyType = propertyName;
        }

        if (className == propertyName)
        {
            propertyName += "Property";
        }

        sb.AppendLine($"\tpublic {propertyType}{(property.Value.Nullable ? "?" : "")} {propertyName} {{ get; init; }}");
        sb.AppendLine();

        return sb;
    }

    private void GenerateOneOf(string ownerName, string nameSpace, OpenApiSchema schema)
    {
        if (schema.OneOf is null)
        {
            return;
        }
        string name = GetClassNameFromKey(ownerName).ToTitleCase();
        StringBuilder baseSb = new StringBuilder();
        GenerateRecord(baseSb, name + "OneOf", nameSpace, name, new OpenApiSchema());
        _generatedOneOfs.Enqueue((name + "OneOf", baseSb.ToString()));
        var classNames = new List<string>(schema.OneOf.Length);

        foreach (OpenApiSchema oneOf in schema.OneOf)
        {
            string className = GetClassNameFromKey(ownerName) + GetClassNameFromKey(oneOf.Title!);
            classNames.Add(className);
            StringBuilder sb = new StringBuilder();

            sb = GenerateRecord(sb, className, nameSpace, oneOf.Title!, oneOf, name + "OneOf");

            _generatedOneOfs.Enqueue((className, sb.ToString()));
        }

        (string converterClassName, string converterSource) = GenerateJsonConvertersForOneOf(name + "OneOf", nameSpace, classNames);


        _generatedOneOfs.Enqueue((converterClassName, converterSource));

    }

    public static (string className, string source) GenerateJsonConvertersForOneOf(string baseClassName, string namespaceName, List<string> classNames)
    {
        StringBuilder sb = new StringBuilder();
        string converterName = baseClassName + "OneOfConverterJson";
        sb.AppendLine("using System.Text.Json;");
        sb.AppendLine("using System.Text.Json.Serialization;");
        sb.AppendLine("using System.Diagnostics;");
        sb.AppendLine("using System.Reflection;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine($"public class {converterName} : JsonConverter<{baseClassName}>");
        sb.AppendLine("{");
        sb.AppendLine();

        foreach (string className in classNames)
        {
            sb.AppendLine($"\tprivate static readonly HashSet<string> _propertiesFor{className} = [];");
            sb.AppendLine();
        }

        sb.AppendLine($"\tprivate readonly Dictionary<string, object?> _valuesFor{baseClassName} = [];");
        sb.AppendLine();

        sb.AppendLine($"\tstatic {converterName}()");
        sb.AppendLine($"\t{{");
        foreach (string className in classNames)
        {
            sb.AppendLine($"\t\tforeach (PropertyInfo prop in typeof({className}).GetProperties())");
            sb.AppendLine($"\t\t{{");
            sb.AppendLine($"\t\t\t_propertiesFor{className}.Add(prop.Name);");
            sb.AppendLine($"\t\t}}");
        }
        sb.AppendLine($"\t}}");

        StringBuilder stringBuilderForMatchProperty = new StringBuilder();
        stringBuilderForMatchProperty.AppendLine();
        foreach (string className in classNames)
        {
            stringBuilderForMatchProperty.AppendLine($"\t\t\t\t\tif (_propertiesFor{className}.Contains(propertyName))");
            stringBuilderForMatchProperty.AppendLine($"\t\t\t\t\t{{");
            stringBuilderForMatchProperty.AppendLine($"\t\t\t\t\t\tif (scopeCount == 0)");
            stringBuilderForMatchProperty.AppendLine($"\t\t\t\t\t\t{{");
            stringBuilderForMatchProperty.AppendLine($"\t\t\t\t\t\t\tresult ??= new {className}();");
            stringBuilderForMatchProperty.AppendLine($"\t\t\t\t\t\t}}");
            stringBuilderForMatchProperty.AppendLine($"\t\t\t\t\t\telse");
            stringBuilderForMatchProperty.AppendLine($"\t\t\t\t\t\t{{");
            stringBuilderForMatchProperty.AppendLine($"\t\t\t\t\t\t\t_valuesFor{baseClassName}.Add(propertyName, null);");
            stringBuilderForMatchProperty.AppendLine($"\t\t\t\t\t\t}}");
            stringBuilderForMatchProperty.AppendLine($"\t\t\t\t\t}}");
        }
        sb.AppendLine();

        //language=cs
        var source =
            $$"""
                  public override {{baseClassName}}? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                  {
                      var scopeCount = 0;
                      string currentProperty = string.Empty;
                      {{baseClassName}}? result = null;
                      while (reader.Read())
                      {
                          switch (reader.TokenType)
                          {
                              case JsonTokenType.StartArray:
                              case JsonTokenType.StartObject:
                                  scopeCount++;
                                  break;
                              case JsonTokenType.EndArray:
                              case JsonTokenType.EndObject:
                              {
                                  if (scopeCount == 0)
                                  {
                                      if (result is null)
                                      {
                                          throw new ArgumentException("We failed to determine the type of the OneOf. This is likely due to an invalid JSON structure or missing properties.");
                                      }

                                      Type resultType = result.GetType();
                                      foreach (var pair in _valuesFor{{baseClassName}})
                                      {
                                          PropertyInfo? property = resultType.GetProperty(pair.Key);
                                          if (property is null)
                                          {
                                              property = resultType.GetProperties().FirstOrDefault(x => x.GetCustomAttributes<JsonPropertyNameAttribute>().FirstOrDefault(attr => attr.Name == pair.Key) is not null);
                                              if (property is null)
                                                  throw new ArgumentException($"Property '{pair.Key}' not found in type '{resultType.Name}'.");
                                          }
                                          property.SetValue(result, pair.Value);
                                      }

                                      return result;
                                  }
                                  scopeCount--;
                                  break;
                              }
                              case JsonTokenType.PropertyName:
                              {
                                  string propertyName = reader.GetString()!;
                                  currentProperty = propertyName;
                                  {{stringBuilderForMatchProperty}}
                                  break;
                              }
                              case JsonTokenType.None:
                              case JsonTokenType.Comment:
                                  break;
                              case JsonTokenType.String:
                                  _valuesForBeskeddataOneOf[currentProperty] = reader.GetString();
                                  break;
                              case JsonTokenType.Number:
                                  _valuesForBeskeddataOneOf[currentProperty] = reader.GetDouble();
                                  break;
                              case JsonTokenType.True:
                                  _valuesForBeskeddataOneOf[currentProperty] = true;
                                  break;
                              case JsonTokenType.False:
                                  _valuesForBeskeddataOneOf[currentProperty] = false;
                                  break;
                              case JsonTokenType.Null:
                                  _valuesForBeskeddataOneOf[currentProperty] = null;
                                  break;
                              default:
                                  throw new ArgumentOutOfRangeException();
                          }
                      }

                      throw new UnreachableException("The JSON reader did not complete reading the OneOf structure. This may indicate an incomplete or malformed JSON input.");
                  }

                  public override void Write(Utf8JsonWriter writer, BeskeddataOneOf value, JsonSerializerOptions options)
                  {
                      throw new NotImplementedException();
                  }  
              }
              """;

        sb.Append(source);

        return ($"{converterName}", sb.ToString());
    }

}