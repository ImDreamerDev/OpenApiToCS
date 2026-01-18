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

        var replacements = new Dictionary<string, string>
        {
            ["namespace"] = namespaceName,
            ["title"] = Document.Info.Title,
            ["className"] = className,
            ["operations"] = operationsSb.ToString(),
            ["errorHandling"] = TemplateEngine.RenderTemplate("ErrorHandling", new Dictionary<string, string>())
        };
        
        string source = TemplateEngine.RenderTemplate("ApiClient", replacements);
        result.Add(className, source);

        return result;
    }

    private Dictionary<string, string> GeneratePolyApiClasses()
    {
        var result = new Dictionary<string, string>();
        char version = Document.Info.Version[0];
        string namespaceName = GetClassNameFromKey(Document.Info.Title).ToTitleCase() + "ApiClientV" + version;
        var groups = Document.Paths
            .GroupBy(path => path.Key.Split('/')[1]) // Group by the first segment of the path
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

            var replacements = new Dictionary<string, string>
            {
                ["namespace"] = namespaceName,
                ["title"] = group.Key,
                ["className"] = className,
                ["operations"] = operationsSb.ToString(),
                ["errorHandling"] = TemplateEngine.RenderTemplate("ErrorHandling", new Dictionary<string, string>())
            };
            
            string source = TemplateEngine.RenderTemplate("ApiClient", replacements);
            result.Add(className, source);
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
        var successfulContent = okResponse.Value?.Content?.FirstOrDefault();
        
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
                    canBeNull = successfulContent.Value.Value.Schema.Nullable;
                }
            }
            if (okResponse.Value?.Content is not null)
            {
                ArgumentNullException.ThrowIfNull(successfulContent);
                string returnType = GetTypeFromKey(successfulContent.Value.Value.Schema);
                if (successfulContent.Value.Value.Schema.Reference is not null)
                {
                    var schema = GetSchemaFromReference(successfulContent.Value.Value.Schema.Reference);

                    if (schema is not null && schema.Type is not "object")
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
            var requestBody = operation.RequestBody.Content.FirstOrDefault();
            if (requestBody.Value.Schema.Reference is not null)
            {
                string typeName = GetClassNameFromKey(requestBody.Value.Schema.Reference).ToTitleCase();
                bodyName = typeName.FirstCharToLower();
                parameters.Add($"{typeName} {bodyName}");
            }
            else if (requestBody.Value.Schema.Type is not null)
            {
                string typeName = GetClassNameFromKey(requestBody.Value.Schema.Type).ToTitleCase();
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
        StringBuilder serializerSetup = new StringBuilder();
        if (hasReturnType || bodyName is not null)
        {
            serializerSetup.AppendLine("\t\tif (jsonSerializerOptions is null)");
            serializerSetup.AppendLine("\t\t{");
            serializerSetup.AppendLine("\t\t\tjsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);");
            serializerSetup.AppendLine("\t\t\tjsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());");
            serializerSetup.AppendLine("\t\t}");
            
            foreach (OneOfConverter oneOfConverter in dataClassGenerationResult.Converters)
            {
                serializerSetup.AppendLine($"\t\tjsonSerializerOptions.Converters.Add(new {oneOfConverter.Name}());");
            }
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

                if (parameter.Required is false)
                {
                    queryBuilder.AppendLine($"\t\tif ({parameter.Name.FirstCharToLower()} is not null)");
                }

                var schema = parameter.Schema;
                if (IsReferenceType(schema) || parameter.Required is true)
                {
                    queryBuilder.AppendLine($"\t\t\tqueryBuilder.Add(\"{parameter.Name}\", {parameter.Name.FirstCharToLower()}.ToString());");
                }
                else
                {
                    queryBuilder.AppendLine($"\t\t\tqueryBuilder.Add(\"{parameter.Name}\", {parameter.Name.FirstCharToLower()}.Value.ToString());");
                }
            }
        }

        // Build response handling
        StringBuilder responseHandling = new StringBuilder();
        if (okResponse.Value?.Content != null && okResponse.Value.Content.Count != 0)
        {
            ArgumentNullException.ThrowIfNull(successfulContent);
            string returnType = GetTypeFromKey(successfulContent.Value.Value.Schema);
            if (successfulContent.Value.Value.Schema.Reference is not null)
            {
                var schema = GetSchemaFromReference(successfulContent.Value.Value.Schema.Reference);

                if (schema is not null && schema.Type is not "object")
                {
                    returnType = GetTypeFromKey(schema, successfulContent.Value.Value.Schema.Reference.Split("/")[^1]);
                }
            }

            responseHandling.AppendLine($"\t\t\tvar content = await response.Content.ReadAsStringAsync();");
            responseHandling.AppendLine($"\t\t\tvar result = JsonSerializer.Deserialize<{returnType}>(content, jsonSerializerOptions);");
            responseHandling.AppendLine("\t\t\tif (result is null && allowNullOrEmptyResponse)");
            responseHandling.AppendLine("\t\t\t{");
            responseHandling.AppendLine(successfulContent.Value.Value.Schema.Type == "array" ? "\t\t\t\treturn [];" : "\t\t\t\treturn null!;");
            responseHandling.AppendLine("\t\t\t}");
            responseHandling.AppendLine("\t\t\treturn result ?? throw new InvalidOperationException(\"Failed to deserialize response.\");");
        }
        else
        {
            responseHandling.AppendLine("\t\t\treturn;");
        }

        var replacements = new Dictionary<string, string>
        {
            ["metadata"] = EmitMetadata ? GenerateMetadata(new StringBuilder(), path, operation).ToString() : string.Empty,
            ["summary"] = GenerateSummaryString(operation.Summary),
            ["returnType"] = returnTypeString,
            ["methodName"] = method + methodName,
            ["parameters"] = string.Join(", ", allParameters),
            ["serializerSetup"] = serializerSetup.ToString(),
            ["queryBuilder"] = queryBuilder.ToString(),
            ["httpMethod"] = method,
            ["path"] = path.Remove(0, 1),
            ["originalPath"] = path,
            ["apiVersionHeader"] = hasApiVersionHeader ? $"\t\thttpRequest.Headers.Add(\"api-version\", \"{Document.Info.Version}\");\n" : string.Empty,
            ["requestBody"] = bodyName is not null ? $"\t\thttpRequest.Content = JsonContent.Create({bodyName}, options: jsonSerializerOptions);\n" : string.Empty,
            ["responseHandling"] = responseHandling.ToString()
        };

        return TemplateEngine.RenderTemplate("ApiOperation", replacements);
    }

    private static string GenerateErrorHandling()
    {
        return TemplateEngine.RenderTemplate("ErrorHandling", new Dictionary<string, string>());
    }

    private static string GetMonoMethodNameFromPath(string? key)
    {

        //I want to have to two last pars of the string /individer/{id}/forbrugssteder/mapped



        if (string.IsNullOrEmpty(key))
            return "object";

        string[] segments = key.Substring(1).Split('/');
        if (segments.Length < 2)
            return segments[^1].ToTitleCase();

        return segments[^2].Replace("{", "").Replace("}", "").Replace(".", "").Replace(" ", "").Replace("-", "").ToTitleCase() + segments[^1].Replace("{", "").Replace("}", "").Replace(".", "").Replace(" ", "").Replace("-", "").ToTitleCase();
    }

    private static string GetMethodNameFromPath(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return "object";

        int lastSlash = key.LastIndexOf('/');
        int prevSlash = lastSlash > 0 ? key.LastIndexOf('/', lastSlash - 1) : -1;

        string raw;
        if (prevSlash > 0 && key.IndexOf('{', prevSlash + 1, lastSlash - prevSlash - 1) == -1)
        {
            // Extract segment before last and last segment
            string beforeLast = key.Substring(prevSlash + 1, lastSlash - prevSlash - 1);
            string last = key[(lastSlash + 1)..];
            beforeLast = beforeLast.Contains('.') ? beforeLast[(beforeLast.LastIndexOf('.') + 1)..] : beforeLast;
            last = last.Contains('.') ? last[(last.LastIndexOf('.') + 1)..] : last;
            raw = beforeLast + " " + last;
        }
        else
        {
            string last = lastSlash >= 0 ? key[(lastSlash + 1)..] : key;
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