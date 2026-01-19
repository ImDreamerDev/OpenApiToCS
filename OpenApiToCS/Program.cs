using System.Diagnostics;
using System.Text.Json;
using OpenApiToCS;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;

// Parse command-line arguments
var options = ArgumentParser.Parse(args);

if (options.ShowHelp)
{
    HelpText.Show();
    return 0;
}

if (options.ShowVersion)
{
    Console.WriteLine("OpenApiToCS v1.0.0");
    return 0;
}

if (string.IsNullOrEmpty(options.InputFile))
{
    Console.Error.WriteLine("Error: No input file specified.");
    Console.Error.WriteLine("Usage: openapitocs <input-file> [options]");
    Console.Error.WriteLine("Try 'openapitocs --help' for more information.");
    return 1;
}

if (!File.Exists(options.InputFile))
{
    Console.Error.WriteLine($"Error: Input file not found: {options.InputFile}");
    return 1;
}

// Load configuration file if it exists
var config = ConfigurationLoader.Load(options.ConfigFile);
if (config != null)
{
    options = ConfigurationLoader.MergeWithConfig(options, config);
}

// Set custom template directory if specified
if (!string.IsNullOrEmpty(options.TemplateDirectory))
{
    if (!Directory.Exists(options.TemplateDirectory))
    {
        Console.Error.WriteLine($"Error: Template directory not found: {options.TemplateDirectory}");
        return 1;
    }
    TemplateEngine.SetCustomTemplateDirectory(options.TemplateDirectory);
}

// Run generator
var exitCode = await CodeGenerator.Generate(options);

// Watch mode
if (exitCode == 0 && options.WatchMode)
{
    await WatchForChanges(options);
}

return exitCode;

static async Task WatchForChanges(CliOptions options)
{
    Console.WriteLine($"\nWatching {options.InputFile} for changes... (Press Ctrl+C to exit)");
    using var watcher = new FileSystemWatcher(Path.GetDirectoryName(options.InputFile) ?? ".")
    {
        Filter = Path.GetFileName(options.InputFile),
        NotifyFilter = NotifyFilters.LastWrite
    };

    watcher.Changed += async (sender, e) =>
    {
        Console.WriteLine($"\n{DateTime.Now:HH:mm:ss} File changed, regenerating...");
        await CodeGenerator.Generate(options);
    };

    watcher.EnableRaisingEvents = true;

    var exitEvent = new ManualResetEvent(false);
    Console.CancelKeyPress += (sender, e) =>
    {
        e.Cancel = true;
        exitEvent.Set();
    };
    exitEvent.WaitOne();
}

static class CodeGenerator
{
    public static async Task<int> Generate(CliOptions options)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var document = await LoadOpenApiDocument(options.InputFile!);
            if (document == null)
                return 1;

            if (options.Validate)
            {
                ValidateDocument(document);
            }

            if (options.Analyze)
            {
                return AnalyzeDocument(document);
            }

            if (options.GenerateMockServer)
            {
                return GenerateMockServer(document, options);
            }

            return await GenerateApiClient(document, options, stopwatch);
        }
        catch (JsonException ex)
        {
            PrintJsonError(ex);
            return 1;
        }
        catch (Exception ex)
        {
            PrintError(ex, options.Verbose);
            return 1;
        }
    }

    private static async Task<OpenApiDocument?> LoadOpenApiDocument(string inputFile)
    {
        string jsonContent = await File.ReadAllTextAsync(inputFile);
        var serializationOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };

        var document = JsonSerializer.Deserialize<OpenApiDocument>(jsonContent, serializationOptions);

        if (document == null)
        {
            Console.Error.WriteLine("Error: Failed to deserialize the OpenAPI document.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Possible causes:");
            Console.Error.WriteLine("  • The file is not valid JSON");
            Console.Error.WriteLine("  • The file is not a valid OpenAPI 3.0 specification");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Suggestions:");
            Console.Error.WriteLine("  • Validate your OpenAPI spec at https://editor.swagger.io/");
            Console.Error.WriteLine("  • Ensure the file uses OpenAPI 3.0 format (not Swagger 2.0)");
            Console.Error.WriteLine("  • Check for JSON syntax errors");
        }

        return document;
    }

    private static void ValidateDocument(OpenApiDocument document)
    {
        var analyzer = new SpecAnalyzer(document);
        var analysisReport = analyzer.Analyze();
        analysisReport.Print();

        if (analysisReport.Score < 50)
        {
            Console.WriteLine("⚠️  Warning: Spec quality score is low. Consider fixing errors and warnings before generation.");
            Console.WriteLine();
        }
    }

    private static int AnalyzeDocument(OpenApiDocument document)
    {
        var analyzer = new SpecAnalyzer(document);
        var analysisReport = analyzer.Analyze();
        analysisReport.Print();
        return 0;
    }

    private static int GenerateMockServer(OpenApiDocument document, CliOptions options)
    {
        var mockGenerator = new MockServerGenerator(document);
        mockGenerator.Generate(options.MockServerOutput ?? Path.Combine(options.OutputDirectory, "MockServer"));
        return 0;
    }

    private static async Task<int> GenerateApiClient(OpenApiDocument document, CliOptions options, Stopwatch stopwatch)
    {
        var dataClasses = new DataClassGenerator(document).GenerateDataClasses();
        var apiClasses = new OperationGenerator(document, dataClasses, options.GenerateMonoClients).GenerateApiClasses();
        
        // Generate webhooks if present (OpenAPI 3.1 feature)
        var webhookGenerator = new WebhookGenerator(document, dataClasses);
        var namespaceName = webhookGenerator.GetClassNameFromKey(document.Info.Title) + "ApiClientV" + document.Info.Version[0];
        var webhooks = webhookGenerator.GenerateWebhooks(namespaceName);

        await OutputWriter.WriteGeneratedFiles(options.OutputDirectory, dataClasses, apiClasses, webhooks);

        PrintSuccess(dataClasses.ClassCount, apiClasses.Count, webhooks.Count, options.OutputDirectory, stopwatch.ElapsedMilliseconds, options.Verbose);
        return 0;
    }

    private static void PrintSuccess(int dataClassCount, int apiClientCount, int webhookCount, string outputDir, long elapsedMs, bool verbose)
    {
        if (verbose)
        {
            Console.WriteLine($"✓ Generated {dataClassCount} data classes");
            Console.WriteLine($"✓ Generated {apiClientCount} API clients");
            if (webhookCount > 0)
            {
                Console.WriteLine($"✓ Generated {webhookCount} webhook handlers");
            }
            Console.WriteLine($"✓ Output: {Path.GetFullPath(outputDir)}");
            Console.WriteLine($"✓ Completed in {elapsedMs} ms");
        }
        else
        {
            var webhookMsg = webhookCount > 0 ? $" and {webhookCount} webhook handlers" : "";
            Console.WriteLine($"Generated {dataClassCount} data classes, {apiClientCount} API clients{webhookMsg} in {elapsedMs} ms");
        }
    }

    private static void PrintJsonError(JsonException ex)
    {
        Console.Error.WriteLine($"Error: Invalid JSON in input file.");
        Console.Error.WriteLine($"  {ex.Message}");
        if (ex.LineNumber.HasValue)
        {
            Console.Error.WriteLine($"  Line {ex.LineNumber}, Position {ex.BytePositionInLine}");
        }
    }

    private static void PrintError(Exception ex, bool verbose)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        if (verbose)
        {
            Console.Error.WriteLine(ex.StackTrace);
        }
    }
}