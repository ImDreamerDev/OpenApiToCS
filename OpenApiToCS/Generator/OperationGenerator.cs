using System.Diagnostics;
using System.Net;
using System.Text;
using OpenApiToCS.OpenApi;

namespace OpenApiToCS.Generator;

public class OperationGenerator : BaseGenerator
{
    public static Dictionary<string, string> GenerateApiClasses(OpenApiDocument document)
    {
        
        var result = new Dictionary<string, string>();
        var version = document.Info.Version[0];
        string namespaceName = GetClassNameFromKey(document.Info.Title).ToTitleCase() + "ApiClientV" + version;

        var groups = document.Paths
            .GroupBy(path => path.Key.Split('/')[1]) // Group by the first segment of the path
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var group in groups)
        {
            StringBuilder classSb = new StringBuilder();

            string className = GetClassNameFromKey(group.Key).ToTitleCase() + "ClientV" + version;
            classSb.AppendLine("using System.Diagnostics;");
            classSb.AppendLine("using System.Net.Http.Json;");
            classSb.AppendLine("using System.Text.Json;");
            classSb.AppendLine("using Hellang.Middleware.ProblemDetails;");
            classSb.AppendLine($"using {namespaceName}.Models;");
            classSb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            classSb.AppendLine();
            classSb.AppendLine($"namespace {namespaceName};");
            classSb.AppendLine();
            classSb.AppendLine($"// Generated API class for {group.Key}");
            classSb.AppendLine($"public class {className}(HttpClient httpClient)");
            classSb.AppendLine("{");

            foreach (var path in group.Value)
            {
                if (path.Value.Get is not null)
                {
                    classSb = GenerateOperationCode(classSb, document, path.Key, path.Value.Get, HttpMethod.Get);

                }
                if (path.Value.Post is not null)
                {
                    classSb = GenerateOperationCode(classSb, document, path.Key, path.Value.Post, HttpMethod.Post);
                }
                if (path.Value.Put is not null)
                {
                    classSb = GenerateOperationCode(classSb, document, path.Key, path.Value.Put, HttpMethod.Put);
                }
                if (path.Value.Delete is not null)
                {
                    classSb = GenerateOperationCode(classSb, document, path.Key, path.Value.Delete, HttpMethod.Delete);
                }

                if (path.Value.Patch is not null)
                {
                    classSb = GenerateOperationCode(classSb, document, path.Key, path.Value.Patch, HttpMethod.Patch);
                }

                if (path.Value.Head is not null)
                {
                    classSb = GenerateOperationCode(classSb, document, path.Key, path.Value.Head, HttpMethod.Head);
                }

                if (path.Value.Options is not null)
                {
                    classSb = GenerateOperationCode(classSb, document, path.Key, path.Value.Options, HttpMethod.Options);
                }

                if (path.Value.Trace is not null)
                {
                    classSb = GenerateOperationCode(classSb, document, path.Key, path.Value.Trace, HttpMethod.Trace);
                }
            }

            classSb.Append(GenerateErrorHandling());
            classSb.AppendLine("}");
            result.Add(className, classSb.ToString());
        }

        return result;
    }

    private static StringBuilder GenerateOperationCode(StringBuilder sb, OpenApiDocument document, string path, OpenApiOperation operation, HttpMethod httpMethod)
    {
        string methodName = GetMethodNameFromPath(path).ToTitleCase();
        sb = GenerateMetadata(sb, path, operation);

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
            return sb;
        }

        sb = GenerateSummary(sb, operation.Summary);

        bool hasReturnType;
        var successfulContent = okResponse.Value?.Content?.FirstOrDefault();
        if (okResponse.Value?.Content is not null && okResponse.Value.Content.Count == 0 || (okResponse.Value is null && successResponse.Value is not null))
        {
            sb.Append("\tpublic async Task " + method + methodName + "(");
            hasReturnType = false;
        }
        else
        {
            bool canBeNull = successResponse.Key is HttpStatusCode.Created or HttpStatusCode.Accepted or HttpStatusCode.NoContent;
            if (canBeNull is false && okResponse.Value is not null)
            {
                if (okResponse.Value.Content is not null)
                {
                    ArgumentNullException.ThrowIfNull(successfulContent);
                    canBeNull = successfulContent.Value.Value.Schema.Nullable;
                }
            }
            if (okResponse.Value?.Content is not null)
            {
                ArgumentNullException.ThrowIfNull(successfulContent);
                string returnType = GetTypeFromKey(successfulContent.Value.Value.Schema!);
                sb.Append("\tpublic async Task<" + returnType + (canBeNull ? "?" : "") + ">" + method + methodName + "(");
                hasReturnType = true;
            }
            else
            {
                sb.Append("\tpublic async Task " + method + methodName + "(");
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
                    // Skip version parameters
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
                            parameters.Add($"{GetTypeFromKey(parameter.Schema)} {parameter.Name}");
                        }
                        else
                        {
                            optionalParameters.Add($"{GetTypeFromKey(parameter.Schema)}? {parameter.Name} = null");
                        }
                        break;
                    default:
                        Console.Error.WriteLine($"Warning: Unsupported parameter location '{parameter.In}' for parameter '{parameter.Name}' at path {path}");
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

        foreach (string parameter in parameters)
        {
            sb.Append(parameter + ", ");
        }

        foreach (string parameter in optionalParameters)
        {
            sb.Append(parameter + ", ");
        }

        sb.Append("Action<HttpRequestMessage>? configureRequest = null" + (hasReturnType || bodyName is not null ? ", " : ""));
        if (hasReturnType)
            sb.Append("bool allowNullOrEmptyResponse = false" + (hasReturnType || bodyName is not null ? ", " : ""));
        if (hasReturnType || bodyName is not null)
            sb.Append("JsonSerializerOptions? jsonSerializerOptions = null");
        sb.Append(')');

        sb.AppendLine();
        sb.AppendLine("\t{");
        string queryString = path;
        var isFirst = true;
        if (operation.Parameters is not null)
        {
            foreach (OpenApiParameter parameter in operation.Parameters)
            {
                if (parameter.In != "query")
                    continue;

                queryString += isFirst ? "?" : "&";
                isFirst = false;
                if (parameter.Required)
                {
                    queryString += $"{parameter.Name}={{{parameter.Name}}}";
                }
                else
                {
                    queryString += $"{parameter.Name}={{{parameter.Name} ?? null}}";
                }
            }
        }
        

        sb.AppendLine($"\t\tHttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.{method}, $\"{queryString}\");");
        if (hasApiVersionHeader)
            sb.AppendLine($"\t\thttpRequest.Headers.Add(\"api-version\", \"{document.Info.Version}\");");

        if (bodyName is not null)
        {
            sb.AppendLine($"\t\thttpRequest.Content = JsonContent.Create({bodyName}, options: jsonSerializerOptions);");
        }
        sb.AppendLine("\t\tconfigureRequest?.Invoke(httpRequest);");
        sb.AppendLine("\t\tHttpResponseMessage response = await httpClient.SendAsync(httpRequest);");
        sb.AppendLine("\t\tif (response.IsSuccessStatusCode)");
        sb.AppendLine("\t\t{");

        if (okResponse.Value?.Content != null && okResponse.Value.Content.Count != 0)
        {
            ArgumentNullException.ThrowIfNull(successfulContent);
            string returnType = GetTypeFromKey(successfulContent.Value.Value.Schema);
            sb.AppendLine($"\t\t\tvar result = await response.Content.ReadFromJsonAsync<{returnType}>(options: jsonSerializerOptions);");
            sb.AppendLine("\t\t\tif (result is null && allowNullOrEmptyResponse)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine(successfulContent.Value.Value.Schema.Type == "array" ? "\t\t\t\treturn [];" : "\t\t\t\treturn null!;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\treturn result ?? throw new InvalidOperationException(\"Failed to deserialize response.\");");
        }
        else
        {
            sb.AppendLine("\t\t\treturn;");

        }
        sb.AppendLine("\t\t}");

        sb.AppendLine($"\t\tawait HandleError(response, $\"{path}\");");
        sb.AppendLine("\t\tthrow new UnreachableException(\"This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.\");");

        sb.AppendLine("\t}");
        sb.AppendLine();
        return sb;
    }

    private static string GenerateErrorHandling()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("\tprivate static async Task HandleError(HttpResponseMessage response, string path)");
        sb.AppendLine("\t{");
        sb.AppendLine("\t\tstring errorContent = await response.Content.ReadAsStringAsync();");
        sb.AppendLine("\t\tProblemDetails? problemDetails = JsonSerializer.Deserialize<ProblemDetails>(errorContent);");
        sb.AppendLine("\t\tif (problemDetails is not null)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tthrow new ProblemDetailsException(problemDetails);");
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t\tif (string.IsNullOrEmpty(errorContent) is false)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tproblemDetails = new ProblemDetails()");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tStatus = (int?)response.StatusCode,");
        sb.AppendLine("\t\t\t\tTitle = $\"Call to {path} failed with status code {response.StatusCode}\",");
        sb.AppendLine("\t\t\t\tDetail = errorContent");
        sb.AppendLine("\t\t\t};");
        sb.AppendLine("\t\t\tthrow new ProblemDetailsException(problemDetails);");
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t\tresponse.EnsureSuccessStatusCode();");
        sb.AppendLine("\t}");
        return sb.ToString();
    }

    /*private static string GetMethodNameFromPath(string? key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return "object"; // Default type if key is null or empty
        }
        string[] split = key.Split('/');
        StringBuilder sb;
        if (split.Length > 2 && split[^2].Contains('{') is false)
        {
            sb = new StringBuilder(split[^2].Split('.')[^1] + " " + split[^1].Split('.')[^1]);
            return sb.Replace("{", "").Replace("}", "").Replace(".", " ").Replace("-", " ").ToString();
        }

        sb = new StringBuilder(split[^1]);

        return sb.Replace(".", " ").Replace("-", " ").Replace("{", "").Replace("}", "").ToString();
    }*/
    
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

        return string.Create(raw.Length, raw, (span, src) =>
        {
            for (int i = 0; i < src.Length; i++)
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