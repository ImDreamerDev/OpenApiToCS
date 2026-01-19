using System.Text;
using OpenApiToCS.Generator.Models;
using OpenApiToCS.OpenApi;

namespace OpenApiToCS.Generator;

public class WebhookGenerator(OpenApiDocument document, DataClassGenerationResult dataClassGenerationResult) : BaseGenerator(document)
{
    private readonly DataClassGenerationResult _dataClasses = dataClassGenerationResult;

    public Dictionary<string, string> GenerateWebhooks(string namespaceName)
    {
        var result = new Dictionary<string, string>();
        
        if (Document.Webhooks == null || Document.Webhooks.Count == 0)
        {
            return result;
        }

        // Use API-specific names to avoid conflicts when generating multiple APIs
        var baseName = GetClassNameFromKey(Document.Info.Title).ToTitleCase();
        var interfaceName = $"I{baseName}WebhookHandler";
        var className = $"{baseName}WebhookHandler";

        var interfaceCode = GenerateWebhookInterface(namespaceName, interfaceName);
        if (!string.IsNullOrEmpty(interfaceCode))
        {
            result.Add(interfaceName, interfaceCode);
        }

        var handlerCode = GenerateWebhookHandler(namespaceName, className, interfaceName);
        if (!string.IsNullOrEmpty(handlerCode))
        {
            result.Add(className, handlerCode);
        }

        return result;
    }

    private string GenerateWebhookInterface(string namespaceName, string interfaceName)
    {
        if (Document.Webhooks == null || Document.Webhooks.Count == 0)
        {
            return string.Empty;
        }

        var methodsSb = new StringBuilder();

        foreach (var webhook in Document.Webhooks)
        {
            var webhookName = webhook.Key;
            var webhookPath = webhook.Value;

            // Process POST operations (webhooks are typically POST)
            if (webhookPath.Post != null)
            {
                var method = GenerateWebhookMethod(webhookName, webhookPath.Post, "Post");
                if (!string.IsNullOrEmpty(method))
                {
                    methodsSb.AppendLine(method);
                }
            }

            // Also support PUT and PATCH if defined
            if (webhookPath.Put != null)
            {
                var method = GenerateWebhookMethod(webhookName, webhookPath.Put, "Put");
                if (!string.IsNullOrEmpty(method))
                {
                    methodsSb.AppendLine(method);
                }
            }

            if (webhookPath.Patch != null)
            {
                var method = GenerateWebhookMethod(webhookName, webhookPath.Patch, "Patch");
                if (!string.IsNullOrEmpty(method))
                {
                    methodsSb.AppendLine(method);
                }
            }
        }

        var replacements = new Dictionary<string, string>
        {
            ["namespace"] = namespaceName,
            ["interfaceName"] = interfaceName,
            ["methods"] = methodsSb.ToString()
        };

        return TemplateEngine.RenderTemplate("WebhookInterface", replacements);
    }

    private string GenerateWebhookMethod(string webhookName, OpenApiOperation operation, string httpMethod)
    {
        var methodName = operation.OperationId ?? (httpMethod + GetClassNameFromKey(webhookName).ToTitleCase());
        
        // Ensure method name starts with capital letter (PascalCase)
        if (!string.IsNullOrEmpty(methodName) && char.IsLower(methodName[0]))
        {
            methodName = char.ToUpper(methodName[0]) + methodName.Substring(1);
        }
        
        // Get request body type if exists
        string? bodyType = null;
        if (operation.RequestBody?.Content != null)
        {
            var content = operation.RequestBody.Content.FirstOrDefault(c => c.Key.Contains("json"));
            if (content.Value?.Schema != null)
            {
                bodyType = GetTypeFromSchema(content.Value.Schema);
            }
        }

        string parameters = bodyType != null ? $"{bodyType} payload" : "";
        
        var summary = !string.IsNullOrEmpty(operation.Summary) 
            ? $"\t/// <summary>\n\t/// {operation.Summary}\n\t/// </summary>\n" 
            : "";

        return $"{summary}\tTask {methodName}Async({parameters});";
    }

    private string GenerateWebhookHandler(string namespaceName, string className, string interfaceName)
    {
        if (Document.Webhooks == null || Document.Webhooks.Count == 0)
        {
            return string.Empty;
        }

        var methodsSb = new StringBuilder();

        foreach (var webhook in Document.Webhooks)
        {
            var webhookName = webhook.Key;
            var webhookPath = webhook.Value;

            if (webhookPath.Post != null)
            {
                var method = GenerateWebhookHandlerMethod(webhookName, webhookPath.Post, "Post");
                if (!string.IsNullOrEmpty(method))
                {
                    methodsSb.AppendLine(method);
                }
            }

            if (webhookPath.Put != null)
            {
                var method = GenerateWebhookHandlerMethod(webhookName, webhookPath.Put, "Put");
                if (!string.IsNullOrEmpty(method))
                {
                    methodsSb.AppendLine(method);
                }
            }

            if (webhookPath.Patch != null)
            {
                var method = GenerateWebhookHandlerMethod(webhookName, webhookPath.Patch, "Patch");
                if (!string.IsNullOrEmpty(method))
                {
                    methodsSb.AppendLine(method);
                }
            }
        }

        var replacements = new Dictionary<string, string>
        {
            ["namespace"] = namespaceName,
            ["className"] = className,
            ["interfaceName"] = interfaceName,
            ["methods"] = methodsSb.ToString()
        };

        return TemplateEngine.RenderTemplate("WebhookHandler", replacements);
    }

    private string GenerateWebhookHandlerMethod(string webhookName, OpenApiOperation operation, string httpMethod)
    {
        var methodName = operation.OperationId ?? (httpMethod + GetClassNameFromKey(webhookName).ToTitleCase());
        
        // Ensure method name starts with capital letter (PascalCase)
        if (!string.IsNullOrEmpty(methodName) && char.IsLower(methodName[0]))
        {
            methodName = char.ToUpper(methodName[0]) + methodName.Substring(1);
        }
        
        // Get request body type if exists
        string? bodyType = null;
        if (operation.RequestBody?.Content != null)
        {
            var content = operation.RequestBody.Content.FirstOrDefault(c => c.Key.Contains("json"));
            if (content.Value?.Schema != null)
            {
                bodyType = GetTypeFromSchema(content.Value.Schema);
            }
        }

        string parameters = bodyType != null ? $"{bodyType} payload" : "";
        
        var summary = !string.IsNullOrEmpty(operation.Summary) 
            ? $"\t/// <summary>\n\t/// {operation.Summary}\n\t/// </summary>\n" 
            : "";

        var logMessage = bodyType != null 
            ? $"Console.WriteLine($\"Received webhook: {methodName} with payload: {{payload}}\");"
            : $"Console.WriteLine(\"Received webhook: {methodName}\");";

        return $"{summary}\tpublic virtual Task {methodName}Async({parameters})\n\t{{\n\t\t{logMessage}\n\t\treturn Task.CompletedTask;\n\t}}";
    }

    private string GetTypeFromSchema(OpenApiSchema schema)
    {
        if (schema.Reference != null)
        {
            return GetClassNameFromKey(schema.Reference);
        }

        var primaryType = schema.GetPrimaryType();
        if (primaryType == "object" && schema.Properties?.Count > 0)
        {
            return "object"; // Anonymous object
        }

        return GetTypeFromKey(schema, "");
    }
}
