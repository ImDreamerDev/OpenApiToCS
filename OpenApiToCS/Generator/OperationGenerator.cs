using System.Net;
using System.Text;
using OpenApiToCS.Generator.Models;
using OpenApiToCS.OpenApi;

namespace OpenApiToCS.Generator;

public class OperationGenerator(OpenApiDocument document, DataClassGenerationResult dataClassGenerationResult, bool monoClient = false) : BaseGenerator(document)
{
    public Dictionary<string, string> GenerateApiClasses()
    {
        return monoClient ? GenerateMonoApiClass() : GeneratePolyApiClasses();
    }


    private Dictionary<string, string> GenerateMonoApiClass()
    {
        var result = new Dictionary<string, string>();
        char version = Document.Info.Version[0];
        string namespaceName = GetClassNameFromKey(Document.Info.Title).ToTitleCase() + "ApiClientV" + version;
        StringBuilder operationsSb = new StringBuilder();

        string className = GetClassNameFromKey(Document.Info.Title).ToTitleCase() + "ClientV" + version;

        foreach (var path in Document.Paths)
        {
            if (path.Value.Get is not null)
            {
                operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Get, HttpMethod.Get));
            }
            if (path.Value.Post is not null)
            {
                operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Post, HttpMethod.Post));
            }
            if (path.Value.Put is not null)
            {
                operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Put, HttpMethod.Put));
            }
            if (path.Value.Delete is not null)
            {
                operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Delete, HttpMethod.Delete));
            }
            if (path.Value.Patch is not null)
            {
                operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Patch, HttpMethod.Patch));
            }
            if (path.Value.Head is not null)
            {
                operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Head, HttpMethod.Head));
            }
            if (path.Value.Options is not null)
            {
                operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Options, HttpMethod.Options));
            }
            if (path.Value.Trace is not null)
            {
                operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Trace, HttpMethod.Trace));
            }
        }

        var securityGenerator = new SecurityGenerator(Document);
        var hasSecuritySchemes = Document.Components?.SecuritySchemes != null && Document.Components.SecuritySchemes.Count > 0;

        string source;
        if (hasSecuritySchemes)
        {
            var optionsClassName = className + "Options";
            var replacements = new Dictionary<string, string>
            {
                ["namespace"] = namespaceName,
                ["title"] = Document.Info.Title,
                ["className"] = className,
                ["optionsClassName"] = optionsClassName,
                ["operations"] = operationsSb.ToString(),
                ["errorHandling"] = TemplateEngine.RenderTemplate("ErrorHandling", new Dictionary<string, string>())
            };

            source = TemplateEngine.RenderTemplate("ApiClient", replacements);
            result.Add(className, source);

            // Generate options class
            var optionsClass = securityGenerator.GenerateOptionsClass(namespaceName, className);
            if (!string.IsNullOrEmpty(optionsClass))
            {
                result.Add(optionsClassName, optionsClass);
            }
        }
        else
        {
            // No security schemes - use simple template
            var replacements = new Dictionary<string, string>
            {
                ["namespace"] = namespaceName,
                ["title"] = Document.Info.Title,
                ["className"] = className,
                ["operations"] = operationsSb.ToString(),
                ["errorHandling"] = TemplateEngine.RenderTemplate("ErrorHandling", new Dictionary<string, string>())
            };

            source = TemplateEngine.RenderTemplate("ApiClientSimple", replacements);
            result.Add(className, source);
        }

        return result;
    }

    private Dictionary<string, string> GeneratePolyApiClasses()
    {
        var result = new Dictionary<string, string>();
        char version = Document.Info.Version[0];
        string namespaceName = GetClassNameFromKey(Document.Info.Title).ToTitleCase() + "ApiClientV" + version;
        var groups = Document.Paths
            .GroupBy(path => 
            {
                var segments = path.Key.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length == 0)
                    return "Api";
                
                // If first segment is "api", use the second segment for grouping
                if (segments.Length > 1 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
                    return segments[1];
                
                // Otherwise use the first segment
                return segments[0];
            })
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var group in groups)
        {
            StringBuilder operationsSb = new StringBuilder();

            string className = GetClassNameFromKey(group.Key).ToTitleCase() + "ClientV" + version;

            foreach (var path in group.Value)
            {
                if (path.Value.Get is not null)
                {
                    operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Get, HttpMethod.Get));
                }
                if (path.Value.Post is not null)
                {
                    operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Post, HttpMethod.Post));
                }
                if (path.Value.Put is not null)
                {
                    operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Put, HttpMethod.Put));
                }
                if (path.Value.Delete is not null)
                {
                    operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Delete, HttpMethod.Delete));
                }
                if (path.Value.Patch is not null)
                {
                    operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Patch, HttpMethod.Patch));
                }
                if (path.Value.Head is not null)
                {
                    operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Head, HttpMethod.Head));
                }
                if (path.Value.Options is not null)
                {
                    operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Options, HttpMethod.Options));
                }
                if (path.Value.Trace is not null)
                {
                    operationsSb.Append(GenerateOperationCode(path.Key, path.Value.Trace, HttpMethod.Trace));
                }
            }

            var securityGenerator = new SecurityGenerator(Document);
            var hasSecuritySchemes = Document.Components?.SecuritySchemes != null && Document.Components.SecuritySchemes.Count > 0;

            string source;
            if (hasSecuritySchemes)
            {
                var optionsClassName = className + "Options";
                var replacements = new Dictionary<string, string>
                {
                    ["namespace"] = namespaceName,
                    ["title"] = group.Key,
                    ["className"] = className,
                    ["optionsClassName"] = optionsClassName,
                    ["operations"] = operationsSb.ToString(),
                    ["errorHandling"] = TemplateEngine.RenderTemplate("ErrorHandling", new Dictionary<string, string>())
                };

                source = TemplateEngine.RenderTemplate("ApiClient", replacements);
                result.Add(className, source);

                // Generate options class for each client
                var optionsClass = securityGenerator.GenerateOptionsClass(namespaceName, className);
                if (!string.IsNullOrEmpty(optionsClass))
                {
                    result.Add(optionsClassName, optionsClass);
                }
            }
            else
            {
                // No security schemes - use simple template
                var replacements = new Dictionary<string, string>
                {
                    ["namespace"] = namespaceName,
                    ["title"] = group.Key,
                    ["className"] = className,
                    ["operations"] = operationsSb.ToString(),
                    ["errorHandling"] = TemplateEngine.RenderTemplate("ErrorHandling", new Dictionary<string, string>())
                };

                source = TemplateEngine.RenderTemplate("ApiClientSimple", replacements);
                result.Add(className, source);
            }
        }

        return result;
    }

    private string GenerateOperationCode(string path, OpenApiOperation operation, HttpMethod httpMethod)
    {
        string methodName;
        if (monoClient is false)
        {
            methodName = GetMethodNameFromPath(path).ToTitleCase();
        }
        else
        {
            methodName = GetMonoMethodNameFromPath(path).ToTitleCase();
        }

        string method = httpMethod.Method switch
        {
            "GET" => "Get",
            "POST" => "Post",
            "PUT" => "Put",
            "DELETE" => "Delete",
            "PATCH" => "Patch",
            "HEAD" => "Head",
            "OPTIONS" => "Options",
            "TRACE" => "Trace",
            _ => throw new NotSupportedException($"HTTP method {httpMethod.Method} is not supported.")
        };

        var okResponse = operation.Responses.FirstOrDefault(r => r.Key == HttpStatusCode.OK);
        var successResponse = operation.Responses.FirstOrDefault(r => (int)r.Key > 200 && (int)r.Key < 300);

        if (okResponse.Value is null && successResponse.Value is null)
        {
            Console.Error.WriteLine($"Warning: No OK or successful response found operation at path {path}. We are skipping this operation.");
            return string.Empty;
        }

        bool hasReturnType;
        string returnTypeString = string.Empty;
        // Default to application/json if available, otherwise first content type
        var successfulContent = okResponse.Value?.Content?.ContainsKey("application/json") == true
            ? new KeyValuePair<string, OpenApiSchemaContainer>("application/json", okResponse.Value.Content["application/json"])
            : okResponse.Value?.Content?.FirstOrDefault();

        if (okResponse.Value?.Content is not null && okResponse.Value.Content.Count == 0 || (okResponse.Value is null && successResponse.Value is not null))
        {
            hasReturnType = false;
        }
        else
        {
            bool canBeNull = successResponse.Key is HttpStatusCode.Created or HttpStatusCode.Accepted or HttpStatusCode.NoContent;
            if (canBeNull is false && okResponse.Value is not null)
            {
                if (okResponse.Value.Content is not null && canBeNull is false)
                {
                    ArgumentNullException.ThrowIfNull(successfulContent);
                    canBeNull = successfulContent.Value.Value.Schema.IsNullable();
                }
            }
            if (okResponse.Value?.Content is not null)
            {
                ArgumentNullException.ThrowIfNull(successfulContent);
                string returnType = GetTypeFromKey(successfulContent.Value.Value.Schema);
                if (successfulContent.Value.Value.Schema.Reference is not null)
                {
                    var schema = GetSchemaFromReference(successfulContent.Value.Value.Schema.Reference);

                    if (schema is not null && schema.GetPrimaryType() is not "object")
                    {
                        returnType = GetTypeFromKey(schema, successfulContent.Value.Value.Schema.Reference.Split("/")[^1]);
                    }
                }
                returnTypeString = "<" + returnType + (canBeNull ? "?" : "") + ">";
                hasReturnType = true;
            }
            else
            {
                hasReturnType = false;
            }
        }

        var parameters = new List<string>();
        var optionalParameters = new List<string>();
        var hasApiVersionHeader = false;

        if (operation.Parameters is not null && operation.Parameters.Length > 0)
        {
            foreach (OpenApiParameter parameter in operation.Parameters)
            {
                if (parameter is { Name: "api-version", In: "header" })
                {
                    hasApiVersionHeader = true;
                    continue;
                }

                switch (parameter.In)
                {
                    case "path":
                    case "query":
                    case "header":
                        if (parameter.Required)
                        {
                            parameters.Add($"{GetTypeFromKey(parameter.Schema)} {parameter.Name.FirstCharToLower()}");
                        }
                        else
                        {
                            optionalParameters.Add($"{GetTypeFromKey(parameter.Schema)}? {parameter.Name.FirstCharToLower()} = null");
                        }
                        break;
                    default:
                        Console.Error.WriteLine($"Warning: Unsupported parameter location '{parameter.In}' for parameter '{parameter.Name.FirstCharToLower()}' at path {path}");
                        break;
                }
            }
        }

        string? bodyName = null;
        if (operation.RequestBody?.Content.Count > 0)
        {
            // Default to application/json if available, otherwise first content type
            var requestBody = operation.RequestBody.Content.ContainsKey("application/json")
                ? new KeyValuePair<string, OpenApiSchemaContainer>("application/json", operation.RequestBody.Content["application/json"])
                : operation.RequestBody.Content.First();

            if (requestBody.Value.Schema.Reference is not null)
            {
                string typeName = GetClassNameFromKey(requestBody.Value.Schema.Reference).ToTitleCase();
                bodyName = typeName.FirstCharToLower();
                parameters.Add($"{typeName} {bodyName}");
            }
            else if (requestBody.Value.Schema.GetPrimaryType() is not null)
            {
                string typeName = GetClassNameFromKey(requestBody.Value.Schema.GetPrimaryType()!).ToTitleCase();
                bodyName = methodName.FirstCharToLower();
                parameters.Add($"{typeName} {bodyName}");
            }
            else
            {
                Console.Error.WriteLine($"Warning: No schema found for request body at path {path}");
            }
        }

        List<string> allParameters = new List<string>(parameters);
        allParameters.AddRange(optionalParameters);
        allParameters.Add("Action<HttpRequestMessage>? configureRequest = null");

        if (hasReturnType)
            allParameters.Add("bool allowNullOrEmptyResponse = false");
        if (hasReturnType || bodyName is not null)
            allParameters.Add("JsonSerializerOptions? jsonSerializerOptions = null");

        // Build serializer setup
        string serializerSetup = string.Empty;
        if (hasReturnType || bodyName is not null)
        {
            var converterRegistrations = new StringBuilder();
            foreach (OneOfConverter oneOfConverter in dataClassGenerationResult.Converters)
            {
                converterRegistrations.AppendLine($"\t\tjsonSerializerOptions.Converters.Add(new {oneOfConverter.Name}());");
            }

            var serializerReplacements = new Dictionary<string, string>
            {
                ["converterRegistrations"] = converterRegistrations.ToString()
            };
            serializerSetup = TemplateEngine.RenderTemplate("SerializerSetup", serializerReplacements);
        }

        // Build query builder
        StringBuilder queryBuilder = new StringBuilder();
        queryBuilder.AppendLine("\t\tvar queryBuilder = new QueryBuilder();");
        if (operation.Parameters is not null)
        {
            foreach (OpenApiParameter parameter in operation.Parameters)
            {
                if (parameter.In != "query")
                    continue;

                var schema = parameter.Schema;
                var paramValue = IsReferenceType(schema) || parameter.Required is true
                    ? parameter.Name.FirstCharToLower()
                    : $"{parameter.Name.FirstCharToLower()}.Value";

                if (parameter.Required is false)
                {
                    var optionalReplacements = new Dictionary<string, string>
                    {
                        ["paramName"] = parameter.Name.FirstCharToLower(),
                        ["paramOriginalName"] = parameter.Name,
                        ["paramValue"] = paramValue
                    };
                    queryBuilder.Append(TemplateEngine.RenderTemplate("QueryParameterOptional", optionalReplacements));
                }
                else
                {
                    var requiredReplacements = new Dictionary<string, string>
                    {
                        ["paramName"] = parameter.Name,
                        ["paramValue"] = parameter.Name.FirstCharToLower()
                    };
                    queryBuilder.Append(TemplateEngine.RenderTemplate("QueryParameter", requiredReplacements));
                }
            }
        }

        // Build header parameters
        StringBuilder headerBuilder = new StringBuilder();
        if (operation.Parameters is not null)
        {
            foreach (OpenApiParameter parameter in operation.Parameters)
            {
                if (parameter.In != "header")
                    continue;
                
                // Skip api-version header as it's handled separately
                if (parameter.Name == "api-version")
                    continue;

                var schema = parameter.Schema;
                var paramValue = IsReferenceType(schema) || parameter.Required is true
                    ? parameter.Name.FirstCharToLower()
                    : $"{parameter.Name.FirstCharToLower()}.Value";

                if (parameter.Required is false)
                {
                    var optionalReplacements = new Dictionary<string, string>
                    {
                        ["paramName"] = parameter.Name.FirstCharToLower(),
                        ["headerName"] = parameter.Name,
                        ["paramValue"] = paramValue
                    };
                    headerBuilder.Append(TemplateEngine.RenderTemplate("HeaderParameterOptional", optionalReplacements));
                }
                else
                {
                    var requiredReplacements = new Dictionary<string, string>
                    {
                        ["paramName"] = parameter.Name.FirstCharToLower(),
                        ["headerName"] = parameter.Name,
                        ["paramValue"] = parameter.Name.FirstCharToLower()
                    };
                    headerBuilder.Append(TemplateEngine.RenderTemplate("HeaderParameter", requiredReplacements));
                }
            }
        }

        // Build response handling
        string responseHandling;
        if (okResponse.Value?.Content != null && okResponse.Value.Content.Count != 0)
        {
            ArgumentNullException.ThrowIfNull(successfulContent);
            string returnType = GetTypeFromKey(successfulContent.Value.Value.Schema);
            if (successfulContent.Value.Value.Schema.Reference is not null)
            {
                var schema = GetSchemaFromReference(successfulContent.Value.Value.Schema.Reference);

                if (schema is not null && schema.GetPrimaryType() is not null and not "object")
                {
                    returnType = GetTypeFromKey(schema, successfulContent.Value.Value.Schema.Reference.Split("/")[^1]);
                }
            }

            var primaryType = successfulContent.Value.Value.Schema.GetPrimaryType();
            var nullReturn = primaryType == "array" ? "[]" : "null!";
            var responseReplacements = new Dictionary<string, string>
            {
                ["returnType"] = returnType,
                ["nullReturn"] = nullReturn
            };
            responseHandling = TemplateEngine.RenderTemplate("ResponseHandlingWithReturn", responseReplacements);
        }
        else
        {
            responseHandling = TemplateEngine.RenderTemplate("ResponseHandlingVoid", new Dictionary<string, string>());
        }

        var securityGenerator = new SecurityGenerator(Document);
        var securityApplication = securityGenerator.GenerateSecurityApplication();

        var replacements = new Dictionary<string, string>
        {
            ["metadata"] = EmitMetadata ? GenerateMetadata(new StringBuilder(), path, operation).ToString() : string.Empty,
            ["summary"] = GenerateSummaryString(operation.Summary),
            ["returnType"] = returnTypeString,
            ["methodName"] = method + methodName,
            ["parameters"] = string.Join(", ", allParameters),
            ["serializerSetup"] = serializerSetup,
            ["queryBuilder"] = queryBuilder.ToString(),
            ["headerParameters"] = headerBuilder.ToString(),
            ["securityApplication"] = securityApplication,
            ["httpMethod"] = method,
            ["path"] = path.Remove(0, 1),
            ["originalPath"] = path,
            ["apiVersionHeader"] = hasApiVersionHeader ? $"\t\thttpRequest.Headers.Add(\"api-version\", \"{Document.Info.Version}\");\n" : string.Empty,
            ["requestBody"] = bodyName is not null ? $"\t\thttpRequest.Content = JsonContent.Create({bodyName}, options: jsonSerializerOptions);\n" : string.Empty,
            ["responseHandling"] = responseHandling
        };

        return TemplateEngine.RenderTemplate("ApiOperation", replacements);
    }

    private static string GetMonoMethodNameFromPath(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return "object";

        // Skip /api/ prefix if present (case-insensitive)
        string workingPath = key;
        if (workingPath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) || 
            workingPath.StartsWith("/API/", StringComparison.OrdinalIgnoreCase))
        {
            workingPath = workingPath.Substring(4); // Remove "/api" but keep the trailing "/"
        }

        string[] segments = workingPath.Substring(1).Split('/');
        
        // For mono client, use all non-parameter segments to avoid naming collisions
        StringBuilder nameBuilder = new StringBuilder();
        foreach (var segment in segments)
        {
            // Skip parameter segments (those in curly braces)
            if (segment.Contains('{'))
                continue;
                
            string cleaned = segment.Replace("{", "").Replace("}", "").Replace(".", "").Replace(" ", "").Replace("-", "");
            if (!string.IsNullOrEmpty(cleaned))
            {
                nameBuilder.Append(cleaned.ToTitleCase());
            }
        }
        
        string result = nameBuilder.ToString();
        return string.IsNullOrEmpty(result) ? "Operation" : result;
    }

    private static string GetMethodNameFromPath(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return "object";

        // Skip /api/ prefix if present (case-insensitive)
        string workingPath = key;
        if (workingPath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) || 
            workingPath.StartsWith("/API/", StringComparison.OrdinalIgnoreCase))
        {
            workingPath = workingPath.Substring(4); // Remove "/api" but keep the trailing "/"
        }

        int lastSlash = workingPath.LastIndexOf('/');
        int prevSlash = lastSlash > 0 ? workingPath.LastIndexOf('/', lastSlash - 1) : -1;

        string raw;
        if (prevSlash > 0 && workingPath.IndexOf('{', prevSlash + 1, lastSlash - prevSlash - 1) == -1)
        {
            // Extract segment before last and last segment
            string beforeLast = workingPath.Substring(prevSlash + 1, lastSlash - prevSlash - 1);
            string last = workingPath[(lastSlash + 1)..];
            beforeLast = beforeLast.Contains('.') ? beforeLast[(beforeLast.LastIndexOf('.') + 1)..] : beforeLast;
            last = last.Contains('.') ? last[(last.LastIndexOf('.') + 1)..] : last;
            raw = beforeLast + " " + last;
        }
        else
        {
            string last = lastSlash >= 0 ? workingPath[(lastSlash + 1)..] : workingPath;
            raw = last;
        }

        return string.Create(raw.Length,
            raw,
            (span, src) =>
            {
                for (var i = 0; i < src.Length; i++)
                {
                    char c = src[i];
                    span[i] = c switch
                    {
                        '{' or '}' or '.' or '-' => ' ',
                        _ => c
                    };
                }
            }).Trim();
    }
}