using System.Text;
using OpenApiToCS.Generator.Models;
using OpenApiToCS.OpenApi;

namespace OpenApiToCS.Generator;

public class DataClassGenerator : BaseGenerator
{
    private readonly Queue<(string name, OpenApiSchema schema)> _missingSchemasToGenerate = [];
    private readonly HashSet<string> _generatedSchemas = [];
    private readonly DataClassGenerationResult _result = new DataClassGenerationResult();

    public DataClassGenerationResult GenerateDataClasses(OpenApiDocument document)
    {
        string namespaceName = GetClassNameFromKey(document.Info.Title).ToTitleCase() + "ApiClient" + "V" + document.Info.Version[0] + ".Models";
        foreach (var schema in document.Components.Schemas)
        {
            if (schema.Value.Type is not null and not "object" && schema.Value is { Type: not "string", Enum: null, Items: null })
            {
                Console.Error.WriteLine("Unsupported schema type: " + schema.Value.Type + " for key: " + schema.Key);
                continue;
            }
            if (schema.Value.Type is not "object" and not "array" and not "string")
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

            Class @class;

            if (schema.Value.Enum is not null)
            {
                @class = GenerateEnum(className, namespaceName, schema.Key, schema.Value);
                _result.Classes.Add(className, @class);
                continue;
            }
            if (schema.Value.Items is not null && schema.Value.Type == "array" && schema.Value.Items.Type == "object" && schema.Value.Items.Properties is not null && schema.Value.Items.Reference is null)
            {
                @class = GenerateRecord(className, namespaceName, schema.Key, schema.Value.Items);
            }
            else
            {
                @class = GenerateRecord(className, namespaceName, schema.Key, schema.Value);
            }
            _result.Classes.Add(className, @class);
        }

        while (_missingSchemasToGenerate.TryDequeue(out (string name, OpenApiSchema schema) tuple))
        {
            string className = GetClassNameFromKey(tuple.name).ToTitleCase();
            if (_generatedSchemas.Add(className) is false)
            {
                continue; // Already generated
            }

            Class @class = GenerateRecord(className, namespaceName, tuple.name, tuple.schema);
            _result.Classes.Add(className, @class);
        }

        return _result;
    }

    private Class GenerateEnum(string className, string namespaceName, string key, OpenApiSchema schema)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("using System.Text.Json.Serialization;");
        sb.AppendLine($"namespace {namespaceName};");
        sb = GenerateMetadata(sb, key, schema);
        sb = GenerateSummary(sb, schema.Description);
        sb.AppendLine("\t[JsonConverter(typeof(JsonStringEnumConverter))]");
        sb.AppendLine($"\tpublic enum {className}");
        sb.AppendLine("{");
        foreach (object enumValue in schema.Enum!)
        {
            sb.AppendLine($"        {enumValue.ToString()},");
        }
        sb.AppendLine("}");
        return new Class(className, namespaceName, sb.ToString(), [], []);
    }

    private Class GenerateRecord(string className, string namespaceName, string key, OpenApiSchema schema, string? baseClass = null)
    {
        StringBuilder sb = new StringBuilder();
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
        List<Property> properties = [];
        List<OneOfConverter> oneOfConverters = [];
        if (schema.Properties is not null)
        {
            foreach (var property in schema.Properties)
            {
                Property prop = GenerateProperty(sb, property, schema, className);
                properties.Add(prop);

                if (property.Value.Items?.OneOf == null)
                    continue;

                OneOfConverter? converter = GenerateOneOf(property.Key, namespaceName, property.Value.Items);
                if (converter is not null)
                {
                    oneOfConverters.Add(converter);
                }
            }
        }

        sb.AppendLine("}");
        return new Class(className, namespaceName, sb.ToString(), properties, oneOfConverters);
    }

    private Property GenerateProperty(StringBuilder sb, KeyValuePair<string, OpenApiSchema> property, OpenApiSchema schema, string className)
    {
        GenerateSummary(sb, property.Value.Description);

        if (property.Value.Deprecated)
        {
            sb.AppendLine("\t[Obsolete(\"This property is deprecated.\")]");
        }

        bool isRequired = schema.Required is not null && schema.Required.Contains(property.Key);
        if (isRequired)
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
                propertyType = GetTypeFromKey(property.Value, property.Key);
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

        if (property.Value.Items is not null && property.Value.Items.Type == "object" && property.Value.Items.Reference is null && property.Value.Items.OneOf is null)
        {
            _missingSchemasToGenerate.Enqueue((propertyName + "Item", property.Value.Items));
        }

        return new Property(propertyName, propertyType, isRequired, property.Value.Nullable, property.Value.Description);
    }

    private OneOfConverter? GenerateOneOf(string ownerName, string nameSpace, OpenApiSchema schema)
    {
        if (schema.OneOf is null)
        {
            return null;
        }

        string name = GetClassNameFromKey(ownerName).ToTitleCase();
        var oneOfClasses = new List<Class>();

        if (_generatedSchemas.Contains(name + "OneOf"))
        {
            Console.WriteLine($"OneOf class {name + "OneOf"} already generated for {ownerName}. Skipping generation.");
            return null; // Already generated
        }

        Class baseClass = GenerateRecord(name + "OneOf", nameSpace, name, new OpenApiSchema());
        oneOfClasses.Add(baseClass);

        var classNames = new List<string>(schema.OneOf.Length);

        foreach (OpenApiSchema oneOf in schema.OneOf)
        {
            string className = GetClassNameFromKey(ownerName) + GetClassNameFromKey(oneOf.Title!);
            classNames.Add(className);
            Class oneOfClass = GenerateRecord(className, nameSpace, oneOf.Title!, oneOf, name + "OneOf");
            oneOfClasses.Add(oneOfClass);
        }

        (string converterClassName, string converterSource) = GenerateJsonConvertersForOneOf(name + "OneOf", nameSpace, classNames);

        return new OneOfConverter(converterClassName, nameSpace, converterSource, oneOfClasses);
    }

    public static (string className, string source) GenerateJsonConvertersForOneOf(string baseClassName, string namespaceName, List<string> classNames)
    {
        StringBuilder sb = new StringBuilder();
        string converterName = baseClassName + "ConverterJson";
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