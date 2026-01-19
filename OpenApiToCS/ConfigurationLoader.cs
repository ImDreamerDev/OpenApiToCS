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

        // Merge NuGet package configuration
        options.PackageId ??= config.PackageId;
        options.PackageVersion ??= config.PackageVersion;
        options.PackageAuthors ??= config.PackageAuthors;
        options.PackageCompany ??= config.PackageCompany;
        options.PackageDescription ??= config.PackageDescription;
        options.PackageTags ??= config.PackageTags;
        options.PackageRepositoryUrl ??= config.PackageRepositoryUrl;
        options.PackageLicense ??= config.PackageLicense;

        return options;
    }
}
