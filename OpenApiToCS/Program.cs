using System.Diagnostics;
using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;

// Parse command-line arguments
var options = ParseArguments(args);

if (options.ShowHelp)
{
    ShowHelp();
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
var config = LoadConfiguration(options.ConfigFile);
if (config != null)
{
    options = MergeWithConfig(options, config);
}

// Run generator
var exitCode = await GenerateCode(options);

// Watch mode
if (exitCode == 0 && options.WatchMode)
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
        await GenerateCode(options);
    };
    
    watcher.EnableRaisingEvents = true;
    
    // Keep running until Ctrl+C
    var exitEvent = new ManualResetEvent(false);
    Console.CancelKeyPress += (sender, e) =>
    {
        e.Cancel = true;
        exitEvent.Set();
    };
    exitEvent.WaitOne();
}

return exitCode;

static async Task<int> GenerateCode(CliOptions options)
{
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        // Read and parse OpenAPI document
        string jsonContent = await File.ReadAllTextAsync(options.InputFile);
        
        var serializationOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };

        OpenApiDocument? document = JsonSerializer.Deserialize<OpenApiDocument>(jsonContent, serializationOptions);

        if (document is null)
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
            return 1;
        }

        // Validate document
        if (options.Validate)
        {
            var warnings = ValidateDocument(document);
            if (warnings.Count > 0)
            {
                Console.WriteLine("Validation warnings:");
                foreach (var warning in warnings)
                {
                    Console.WriteLine($"  ⚠ {warning}");
                }
                Console.WriteLine();
            }
        }

        // Generate code
        var dataClasses = new DataClassGenerator(document).GenerateDataClasses();
        var apiClasses = new OperationGenerator(document, dataClasses, options.GenerateInterfaces).GenerateApiClasses();

        // Create output directories
        Directory.CreateDirectory(Path.Combine(options.OutputDirectory, "Models"));
        Directory.CreateDirectory(Path.Combine(options.OutputDirectory, "Api"));

        // Write data classes
        int index = 0;
        foreach (var dataClass in dataClasses.Classes)
        {
            var fileName = Environment.OSVersion.Platform is PlatformID.Win32NT or PlatformID.Win32Windows or PlatformID.Win32S
                ? $"{dataClass.Key}{index}.cs"  // Append index on Windows to avoid case-sensitivity issues
                : $"{dataClass.Key}.cs";
            
            await File.WriteAllTextAsync(
                Path.Combine(options.OutputDirectory, "Models", fileName),
                dataClass.Value.Source);
            
            // Write OneOf converters
            foreach (var converter in dataClass.Value.OneOfConverters)
            {
                await File.WriteAllTextAsync(
                    Path.Combine(options.OutputDirectory, "Models", $"{converter.Name}.cs"),
                    converter.Source);
                
                foreach (var oneOf in converter.OneOfs)
                {
                    await File.WriteAllTextAsync(
                        Path.Combine(options.OutputDirectory, "Models", $"{oneOf.Name}.cs"),
                        oneOf.Source);
                }
            }
            index++;
        }

        // Write API clients
        foreach (var apiClass in apiClasses)
        {
            await File.WriteAllTextAsync(
                Path.Combine(options.OutputDirectory, "Api", $"{apiClass.Key}.cs"),
                apiClass.Value);
        }

        if (options.Verbose)
        {
            Console.WriteLine($"✓ Generated {dataClasses.ClassCount} data classes");
            Console.WriteLine($"✓ Generated {apiClasses.Count} API clients");
            Console.WriteLine($"✓ Output: {Path.GetFullPath(options.OutputDirectory)}");
            Console.WriteLine($"✓ Completed in {stopwatch.ElapsedMilliseconds} ms");
        }
        else
        {
            Console.WriteLine($"Generated {dataClasses.ClassCount} data classes and {apiClasses.Count} API clients in {stopwatch.ElapsedMilliseconds} ms");
        }

        return 0;
    }
    catch (JsonException ex)
    {
        Console.Error.WriteLine($"Error: Invalid JSON in input file.");
        Console.Error.WriteLine($"  {ex.Message}");
        if (ex.LineNumber.HasValue)
        {
            Console.Error.WriteLine($"  Line {ex.LineNumber}, Position {ex.BytePositionInLine}");
        }
        return 1;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        if (options.Verbose)
        {
            Console.Error.WriteLine(ex.StackTrace);
        }
        return 1;
    }
}

static List<string> ValidateDocument(OpenApiDocument document)
{
    var warnings = new List<string>();
    
    if (string.IsNullOrWhiteSpace(document.Info?.Title))
        warnings.Add("Document has no title (info.title)");
    
    if (string.IsNullOrWhiteSpace(document.Info?.Version))
        warnings.Add("Document has no version (info.version)");
    
    if (document.Paths == null || document.Paths.Count == 0)
        warnings.Add("Document has no paths defined");
    
    if (document.Components?.Schemas == null || document.Components.Schemas.Count == 0)
        warnings.Add("Document has no schemas defined");
    
    return warnings;
}

static CliOptions ParseArguments(string[] args)
{
    var options = new CliOptions();
    
    for (int i = 0; i < args.Length; i++)
    {
        var arg = args[i];
        
        if (arg == "--help" || arg == "-h")
        {
            options.ShowHelp = true;
            return options;
        }
        
        if (arg == "--version" || arg == "-v")
        {
            options.ShowVersion = true;
            return options;
        }
        
        if (arg == "--output" || arg == "-o")
        {
            if (i + 1 < args.Length)
                options.OutputDirectory = args[++i];
            continue;
        }
        
        if (arg == "--config" || arg == "-c")
        {
            if (i + 1 < args.Length)
                options.ConfigFile = args[++i];
            continue;
        }
        
        if (arg == "--namespace" || arg == "-n")
        {
            if (i + 1 < args.Length)
                options.Namespace = args[++i];
            continue;
        }
        
        if (arg == "--watch" || arg == "-w")
        {
            options.WatchMode = true;
            continue;
        }
        
        if (arg == "--verbose")
        {
            options.Verbose = true;
            continue;
        }
        
        if (arg == "--validate")
        {
            options.Validate = true;
            continue;
        }
        
        if (arg == "--interfaces")
        {
            options.GenerateInterfaces = true;
            continue;
        }
        
        // First non-option argument is the input file
        if (!arg.StartsWith("-") && string.IsNullOrEmpty(options.InputFile))
        {
            options.InputFile = arg;
        }
    }
    
    return options;
}

static Configuration? LoadConfiguration(string? configFile)
{
    configFile ??= "openapitocsconfig.json";
    
    if (!File.Exists(configFile))
        return null;
    
    try
    {
        var json = File.ReadAllText(configFile);
        return JsonSerializer.Deserialize<Configuration>(json);
    }
    catch
    {
        Console.WriteLine($"Warning: Failed to load configuration from {configFile}");
        return null;
    }
}

static CliOptions MergeWithConfig(CliOptions options, Configuration config)
{
    // Command-line options take precedence over config file
    options.OutputDirectory ??= config.OutputDirectory;
    options.Namespace ??= config.Namespace;
    
    if (!options.GenerateInterfaces && config.GenerateInterfaces)
        options.GenerateInterfaces = true;
    
    return options;
}

static void ShowHelp()
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
    Console.WriteLine("  -w, --watch               Watch for changes and regenerate");
    Console.WriteLine("  --validate                Validate OpenAPI spec and show warnings");
    Console.WriteLine("  --interfaces              Generate interfaces for API clients");
    Console.WriteLine("  --verbose                 Show detailed output");
    Console.WriteLine("  -h, --help                Show this help");
    Console.WriteLine("  -v, --version             Show version");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  openapitocs petstore.json");
    Console.WriteLine("  openapitocs api.json -o ./generated --namespace MyApi.Client");
    Console.WriteLine("  openapitocs api.json --watch --verbose");
    Console.WriteLine("  openapitocs api.json --validate --interfaces");
    Console.WriteLine();
    Console.WriteLine("Configuration file (openapitocsconfig.json):");
    Console.WriteLine("  {");
    Console.WriteLine("    \"outputDirectory\": \"./generated\",");
    Console.WriteLine("    \"namespace\": \"MyApi.Client\",");
    Console.WriteLine("    \"generateInterfaces\": true");
    Console.WriteLine("  }");
}

class CliOptions
{
    public string? InputFile { get; set; }
    public string OutputDirectory { get; set; } = "code";
    public string? Namespace { get; set; }
    public string? ConfigFile { get; set; }
    public bool WatchMode { get; set; }
    public bool Verbose { get; set; }
    public bool Validate { get; set; }
    public bool GenerateInterfaces { get; set; }
    public bool ShowHelp { get; set; }
    public bool ShowVersion { get; set; }
}

class Configuration
{
    public string? OutputDirectory { get; set; }
    public string? Namespace { get; set; }
    public bool GenerateInterfaces { get; set; }
}