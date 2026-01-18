using System.Text;
using OpenApiToCS.OpenApi;

namespace OpenApiToCS.Generator;

public class MockServerGenerator
{
    private readonly OpenApiDocument _document;

    public MockServerGenerator(OpenApiDocument document)
    {
        _document = document;
    }

    public void Generate(string outputPath)
    {
        Directory.CreateDirectory(outputPath);

        GenerateProjectFile(outputPath);
        GenerateProgramCs(outputPath);
        GenerateReadme(outputPath);

        Console.WriteLine($"✓ Mock server generated in: {outputPath}");
        Console.WriteLine($"  Run: cd {outputPath} && dotnet run");
    }

    private void GenerateProjectFile(string outputPath)
    {
        var csproj = @"<Project Sdk=""Microsoft.NET.Sdk.Web"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>";

        File.WriteAllText(Path.Combine(outputPath, "MockServer.csproj"), csproj);
    }

    private void GenerateProgramCs(string outputPath)
    {
        var endpoints = new StringBuilder();

        if (_document.Paths != null)
        {
            foreach (var pathItem in _document.Paths)
            {
                GenerateEndpoint(endpoints, pathItem.Key, "GET", pathItem.Value.Get);
                GenerateEndpoint(endpoints, pathItem.Key, "POST", pathItem.Value.Post);
                GenerateEndpoint(endpoints, pathItem.Key, "PUT", pathItem.Value.Put);
                GenerateEndpoint(endpoints, pathItem.Key, "DELETE", pathItem.Value.Delete);
                GenerateEndpoint(endpoints, pathItem.Key, "PATCH", pathItem.Value.Patch);
            }
        }

        var replacements = new Dictionary<string, string>
        {
            ["apiTitle"] = _document.Info.Title,
            ["apiVersion"] = _document.Info.Version,
            ["endpoints"] = endpoints.ToString()
        };

        var programCs = TemplateEngine.RenderTemplate("MockServerProgram", replacements);
        File.WriteAllText(Path.Combine(outputPath, "Program.cs"), programCs);
    }

    private void GenerateEndpoint(StringBuilder sb, string path, string method, OpenApiOperation? operation)
    {
        if (operation == null) return;

        var successResponse = operation.Responses?.FirstOrDefault(r => ((int)r.Key) >= 200 && ((int)r.Key) < 300);
        if (successResponse == null || successResponse.Value.Value == null) return;

        var mockData = GenerateMockResponse(successResponse.Value.Value);

        var replacements = new Dictionary<string, string>
        {
            ["summary"] = operation.Summary ?? method + " " + path,
            ["method"] = method,
            ["path"] = path,
            ["mockData"] = mockData
        };

        sb.AppendLine(TemplateEngine.RenderTemplate("MockServerEndpoint", replacements));
    }

    private string GenerateMockResponse(OpenApiResponse response)
    {
        if (response.Content == null || response.Content.Count == 0)
        {
            return "Results.Ok()";
        }

        var content = response.Content.ContainsKey("application/json")
            ? response.Content["application/json"]
            : response.Content.First().Value;

        if (content.Schema.Type == "array")
        {
            return "new[] { new { id = 1, name = \"Example\" } }";
        }
        else if (content.Schema.Type == "object" || content.Schema.Reference != null)
        {
            return "new { id = 1, status = \"success\", message = \"Mock response\" }";
        }
        else
        {
            return "\"Mock response\"";
        }
    }

    private void GenerateReadme(string outputPath)
    {
        var readme = $@"# Mock Server for {_document.Info.Title}

Auto-generated mock API server for development and testing.

## Quick Start

```bash
dotnet run
```

The server will start on `http://localhost:5000`

## Features

- ✅ All endpoints from the OpenAPI spec
- ✅ Mock responses for successful operations
- ✅ CORS enabled for frontend development
- ✅ Request logging to console

## Generated Endpoints

{GenerateEndpointList()}

## Customization

Edit `Program.cs` to customize mock responses or add business logic.

## Production

⚠️ **This is a mock server for development only!** Do not use in production.
";

        File.WriteAllText(Path.Combine(outputPath, "README.md"), readme);
    }

    private string GenerateEndpointList()
    {
        var sb = new StringBuilder();
        
        if (_document.Paths != null)
        {
            foreach (var pathItem in _document.Paths)
            {
                AddEndpointToList(sb, pathItem.Key, "GET", pathItem.Value.Get);
                AddEndpointToList(sb, pathItem.Key, "POST", pathItem.Value.Post);
                AddEndpointToList(sb, pathItem.Key, "PUT", pathItem.Value.Put);
                AddEndpointToList(sb, pathItem.Key, "DELETE", pathItem.Value.Delete);
                AddEndpointToList(sb, pathItem.Key, "PATCH", pathItem.Value.Patch);
            }
        }

        return sb.ToString();
    }

    private void AddEndpointToList(StringBuilder sb, string path, string method, OpenApiOperation? operation)
    {
        if (operation == null) return;
        sb.AppendLine($"- `{method} {path}` - {operation.Summary ?? "No description"}");
    }
}
