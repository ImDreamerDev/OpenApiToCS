using System.Text;
using OpenApiToCS.Generator.Models;
using OpenApiToCS.OpenApi;

namespace OpenApiToCS.Generator;

public class DataClassGenerator(OpenApiDocument document) : BaseGenerator(document)
{
    private readonly Queue<(string name, OpenApiSchema schema)> _missingSchemasToGenerate = [];
    private readonly HashSet<string> _generatedSchemas = [];
    private readonly DataClassGenerationResult _result = new DataClassGenerationResult();

    public DataClassGenerationResult GenerateDataClasses()
    {
        string namespaceName = GetClassNameFromKey(Document.Info.Title).ToTitleCase() + "ApiClient" + "V" + Document.Info.Version[0] + ".Models";
        foreach (var schema in Document.Components.Schemas)
        {
            if (schema.Value.Type is not null and not "object" && schema.Value is { Type: not "string", Enum: null, Items: null, AllOf: null, OneOf: null, AnyOf: null })
            {
                Console.Error.WriteLine("Unsupported schema type: " + schema.Value.Type + " for key: " + schema.Key);
                continue;
            }
            if (schema.Value.Reference is not null && schema.Value.Type is not "object" and not "array" and not "string")
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

            // Handle oneOf at schema level
            if (schema.Value.OneOf is not null)
            {
                var converter = GenerateOneOfAtRoot(className, namespaceName, schema.Key, schema.Value);
                if (converter is not null)
                {
                    foreach (var oneOfClass in converter.OneOfs)
                    {
                        _result.Classes.Add(oneOfClass.Name, oneOfClass);
                    }
                }
                continue;
            }

            // Handle anyOf at schema level (similar to oneOf)
            if (schema.Value.AnyOf is not null)
            {
                var converter = GenerateAnyOfAtRoot(className, namespaceName, schema.Key, schema.Value);
                if (converter is not null)
                {
                    foreach (var anyOfClass in converter.OneOfs)
                    {
                        _result.Classes.Add(anyOfClass.Name, anyOfClass);
                    }
                }
                continue;
            }

            // Handle allOf at schema level (composition/inheritance)
            if (schema.Value.AllOf is not null)
            {
                @class = GenerateAllOfClass(className, namespaceName, schema.Key, schema.Value);
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
        StringBuilder enumValues = new StringBuilder();
        foreach (object enumValue in schema.Enum!)
        {
            var enumReplacements = new Dictionary<string, string> { ["enumValue"] = enumValue.ToString()! };
            enumValues.Append(TemplateEngine.RenderTemplate("EnumValue", enumReplacements));
        }
        
        var replacements = new Dictionary<string, string>
        {
            ["namespace"] = namespaceName,
            ["className"] = className,
            ["metadata"] = EmitMetadata ? GenerateMetadata(new StringBuilder(), key, schema).ToString() : string.Empty,
            ["summary"] = GenerateSummaryString(schema.Description),
            ["enumValues"] = enumValues.ToString()
        };
        
        string source = TemplateEngine.RenderTemplate("EnumClass", replacements);
        return new Class(className, namespaceName, source, [], []);
    }

    private Class GenerateRecord(string className, string namespaceName, string key, OpenApiSchema schema, string? baseClass = null)
    {
        HashSet<string> usings = new HashSet<string>();
        
        if (schema.Reference is not null)
        {
            usings.Add($"using {namespaceName};");
        }
        if (schema.Required?.Count != 0)
        {
            usings.Add("using System.ComponentModel.DataAnnotations;");
        }

        if (schema.Properties is not null)
        {
            foreach (OpenApiSchema prop in schema.Properties.Values)
            {
                if (prop.Reference is not null)
                {
                    usings.Add($"using {namespaceName};");
                }
            }
        }

        usings.Add("using System.Text.Json.Serialization;");
        
        List<Property> properties = [];
        List<OneOfConverter> oneOfConverters = [];
        StringBuilder propertiesBuilder = new StringBuilder();
        
        if (schema.Properties is not null)
        {
            foreach (var property in schema.Properties)
            {
                Property prop = GenerateProperty(propertiesBuilder, property, schema, className);
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

        var replacements = new Dictionary<string, string>
        {
            ["usings"] = string.Join(Environment.NewLine, usings),
            ["namespace"] = namespaceName,
            ["className"] = className,
            ["baseClass"] = baseClass is not null ? $" : {baseClass}" : string.Empty,
            ["metadata"] = EmitMetadata ? GenerateMetadata(new StringBuilder(), key, schema).ToString() : string.Empty,
            ["summary"] = GenerateSummaryString(schema.Description),
            ["properties"] = propertiesBuilder.ToString()
        };
        
        string source = TemplateEngine.RenderTemplate("RecordClass", replacements);
        return new Class(className, namespaceName, source, properties, oneOfConverters);
    }

    private Property GenerateProperty(StringBuilder sb, KeyValuePair<string, OpenApiSchema> property, OpenApiSchema schema, string className)
    {
        bool isRequired = schema.Required is not null && schema.Required.Contains(property.Key);
        
        string propertyName = property.Key.ToTitleCase();
        string? propertyType;

        if (property.Value.Type is not "object" and not null)
        {
            if (property.Value.Items?.OneOf is not null)
            {
                propertyType = property.Key.ToTitleCase() + "OneOf[]";
            }
            else if (property.Value.Items is not null && property.Value.Items.Reference is not null)
            {
                var itemSchema = GetSchemaFromReference(property.Value.Items.Reference);
                if (itemSchema?.AllOf is not null)
                {
                    if (property.Value.Type is not "array")
                        propertyType = GetTypeFromKey(itemSchema.AllOf.First());
                    else
                        propertyType = GetTypeFromKey(itemSchema.AllOf.First()) + "[]";
                }
                else
                    propertyType = GetTypeFromKey(property.Value, property.Key);
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

        var replacements = new Dictionary<string, string>
        {
            ["summary"] = GenerateSummaryString(property.Value.Description),
            ["deprecated"] = property.Value.Deprecated ? "\t[Obsolete(\"This property is deprecated.\")]\n" : string.Empty,
            ["required"] = isRequired ? "\t[Required]\n" : string.Empty,
            ["jsonPropertyName"] = property.Key,
            ["propertyType"] = propertyType,
            ["nullable"] = property.Value.Nullable ? "?" : string.Empty,
            ["propertyName"] = propertyName
        };
        
        string propertyCode = TemplateEngine.RenderTemplate("Property", replacements);
        sb.Append(propertyCode);

        if (property.Value.Items is not null && property.Value.Items.Type == "object" && property.Value.Items.Reference is null && property.Value.Items.OneOf is null)
        {
            _missingSchemasToGenerate.Enqueue((propertyName, property.Value.Items));
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
        string converterName = baseClassName + "ConverterJson";
        
        StringBuilder propertyHashSets = new StringBuilder();
        StringBuilder staticConstructorBody = new StringBuilder();
        StringBuilder propertyMatching = new StringBuilder();
        
        foreach (string className in classNames)
        {
            var hashSetReplacements = new Dictionary<string, string> { ["className"] = className };
            propertyHashSets.AppendLine(TemplateEngine.RenderTemplate("OneOfPropertyHashSet", hashSetReplacements));
        }

        foreach (string className in classNames)
        {
            var constructorReplacements = new Dictionary<string, string> { ["className"] = className };
            staticConstructorBody.Append(TemplateEngine.RenderTemplate("OneOfStaticConstructor", constructorReplacements));
        }

        foreach (string className in classNames)
        {
            var matchingReplacements = new Dictionary<string, string>
            {
                ["className"] = className,
                ["baseClassName"] = baseClassName
            };
            propertyMatching.Append(TemplateEngine.RenderTemplate("OneOfPropertyMatching", matchingReplacements));
        }

        var replacements = new Dictionary<string, string>
        {
            ["namespace"] = namespaceName,
            ["converterName"] = converterName,
            ["baseClassName"] = baseClassName,
            ["propertyHashSets"] = propertyHashSets.ToString(),
            ["staticConstructorBody"] = staticConstructorBody.ToString(),
            ["propertyMatching"] = propertyMatching.ToString()
        };
        
        string source = TemplateEngine.RenderTemplate("OneOfConverter", replacements);
        return (converterName, source);
    }

    private OneOfConverter? GenerateOneOfAtRoot(string className, string namespaceName, string key, OpenApiSchema schema)
    {
        if (schema.OneOf is null || schema.OneOf.Length == 0)
            return null;

        var oneOfClasses = new List<Class>();
        var classNames = new List<string>();

        // Create base class
        Class baseClass = GenerateRecord(className, namespaceName, key, new OpenApiSchema());
        oneOfClasses.Add(baseClass);
        _generatedSchemas.Add(className);

        // Generate derived classes for each oneOf variant
        for (int i = 0; i < schema.OneOf.Length; i++)
        {
            OpenApiSchema variant = schema.OneOf[i];
            string variantClassName = variant.Title ?? $"{className}Variant{i + 1}";
            variantClassName = GetClassNameFromKey(variantClassName).ToTitleCase();

            classNames.Add(variantClassName);
            Class variantClass = GenerateRecord(variantClassName, namespaceName, variantClassName, variant, className);
            oneOfClasses.Add(variantClass);
            _generatedSchemas.Add(variantClassName);
        }

        (string converterClassName, string converterSource) = GenerateJsonConvertersForOneOf(className, namespaceName, classNames);

        return new OneOfConverter(converterClassName, namespaceName, converterSource, oneOfClasses);
    }

    private OneOfConverter? GenerateAnyOfAtRoot(string className, string namespaceName, string key, OpenApiSchema schema)
    {
        if (schema.AnyOf is null || schema.AnyOf.Length == 0)
            return null;

        var anyOfClasses = new List<Class>();
        var classNames = new List<string>();

        // Create base class
        Class baseClass = GenerateRecord(className, namespaceName, key, new OpenApiSchema());
        anyOfClasses.Add(baseClass);
        _generatedSchemas.Add(className);

        // Generate derived classes for each anyOf variant
        for (int i = 0; i < schema.AnyOf.Length; i++)
        {
            OpenApiSchema variant = schema.AnyOf[i];
            string variantClassName = variant.Title ?? $"{className}Variant{i + 1}";
            variantClassName = GetClassNameFromKey(variantClassName).ToTitleCase();

            classNames.Add(variantClassName);
            Class variantClass = GenerateRecord(variantClassName, namespaceName, variantClassName, variant, className);
            anyOfClasses.Add(variantClass);
            _generatedSchemas.Add(variantClassName);
        }

        (string converterClassName, string converterSource) = GenerateJsonConvertersForOneOf(className, namespaceName, classNames);

        return new OneOfConverter(converterClassName, namespaceName, converterSource, anyOfClasses);
    }

    private Class GenerateAllOfClass(string className, string namespaceName, string key, OpenApiSchema schema)
    {
        if (schema.AllOf is null || schema.AllOf.Length == 0)
            return GenerateRecord(className, namespaceName, key, schema);

        // Merge all properties from allOf schemas
        var mergedProperties = new Dictionary<string, OpenApiSchema>();
        var mergedRequired = new List<string>();
        string? baseClassName = null;
        string? description = schema.Description;

        foreach (var allOfSchema in schema.AllOf)
        {
            OpenApiSchema resolvedSchema;

            // Resolve $ref if present
            if (allOfSchema.Reference is not null)
            {
                var refSchema = GetSchemaFromReference(allOfSchema.Reference);
                if (refSchema is null)
                    continue;
                
                // Use the referenced class as base class (simple inheritance)
                if (baseClassName is null && (refSchema.Properties?.Count ?? 0) > 0)
                {
                    baseClassName = GetClassNameFromKey(allOfSchema.Reference).ToTitleCase();
                    continue; // Don't merge properties from base class
                }
                
                resolvedSchema = refSchema;
            }
            else
            {
                resolvedSchema = allOfSchema;
            }

            // Merge properties
            if (resolvedSchema.Properties is not null && resolvedSchema.Properties.Count > 0)
            {
                foreach (var prop in resolvedSchema.Properties)
                {
                    mergedProperties[prop.Key] = prop.Value;
                }
            }

            // Merge required fields
            if (resolvedSchema.Required is not null && resolvedSchema.Required.Count > 0)
            {
                mergedRequired.AddRange(resolvedSchema.Required);
            }

            // Use description if available
            description ??= resolvedSchema.Description;
        }

        // Create merged schema
        var mergedSchema = new OpenApiSchema
        {
            Type = "object",
            Properties = mergedProperties.Count > 0 ? mergedProperties : null,
            Required = mergedRequired.Count > 0 ? mergedRequired : null,
            Description = description
        };

        return GenerateRecord(className, namespaceName, key, mergedSchema, baseClassName);
    }

}