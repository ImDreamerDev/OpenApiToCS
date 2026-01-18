using System.Text.Json;

namespace OpenApiToCS;

public static class ConfigurationLoader
{
    public static Configuration? Load(string? configFile = null)
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

    public static CliOptions MergeWithConfig(CliOptions options, Configuration config)
    {
        // Command-line options take precedence over config file
        options.OutputDirectory ??= config.OutputDirectory;
        options.Namespace ??= config.Namespace;
        options.TemplateDirectory ??= config.TemplateDirectory;

        if (!options.GenerateMonoClients && config.GenerateMonoClient)
            options.GenerateMonoClients = true;

        return options;
    }
}
