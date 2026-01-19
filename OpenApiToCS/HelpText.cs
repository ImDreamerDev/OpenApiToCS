namespace OpenApiToCS;

public static class HelpText
{
    public static void Show()
    {
        Console.WriteLine("OpenApiToCS - Generate C# API clients from OpenAPI specifications");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  openapitocs <input-file> [options]");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <input-file>              Path to OpenAPI JSON file");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -o, --output <dir>        Output directory (default: ./code)");
        Console.WriteLine("  -n, --namespace <name>    Root namespace for generated code");
        Console.WriteLine("  -c, --config <file>       Configuration file (default: openapitocsconfig.json)");
        Console.WriteLine("  -t, --template-dir <dir>  Custom template directory");
        Console.WriteLine("  -w, --watch               Watch for changes and regenerate");
        Console.WriteLine("  --validate                Validate OpenAPI spec with quality checks");
        Console.WriteLine("  --analyze                 Analyze spec quality and show detailed report");
        Console.WriteLine("  --generate-mock-server    Generate runnable mock API server");
        Console.WriteLine("  --mock-output <dir>       Mock server output directory (default: ./MockServer)");
        Console.WriteLine("  --mono                    Generate a client supporting multiple APIs in one");
        Console.WriteLine("  --verbose                 Show detailed output");
        Console.WriteLine("  -h, --help                Show this help");
        Console.WriteLine("  -v, --version             Show version");
        Console.WriteLine();
        Console.WriteLine("NuGet Package Options:");
        Console.WriteLine("  --package-id <id>         Package ID (default: sanitized API title)");
        Console.WriteLine("  --package-version <ver>   Package version (default: API version)");
        Console.WriteLine("  --package-authors <name>  Package authors (default: Generated)");
        Console.WriteLine("  --package-company <name>  Package company (default: Generated)");
        Console.WriteLine("  --package-description <d> Package description");
        Console.WriteLine("  --package-tags <tags>     Package tags (default: openapi;api-client;generated)");
        Console.WriteLine("  --package-repo-url <url>  Repository URL");
        Console.WriteLine("  --package-license <lic>   Package license (default: MIT)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  openapitocs petstore.json");
        Console.WriteLine("  openapitocs api.json -o ./generated --namespace MyApi.Client");
        Console.WriteLine("  openapitocs api.json --watch --verbose");
        Console.WriteLine("  openapitocs api.json --analyze");
        Console.WriteLine("  openapitocs api.json --generate-mock-server");
        Console.WriteLine();
        Console.WriteLine("Configuration file (openapitocsconfig.json):");
        Console.WriteLine("  {");
        Console.WriteLine("    \"outputDirectory\": \"./generated\",");
        Console.WriteLine("    \"namespace\": \"MyApi.Client\",");
        Console.WriteLine("    \"generateMonoClient\": true,");
        Console.WriteLine("    \"packageId\": \"MyApi.Client\",");
        Console.WriteLine("    \"packageVersion\": \"1.0.0\",");
        Console.WriteLine("    \"packageAuthors\": \"Your Name\"");
        Console.WriteLine("  }");
    }
}
