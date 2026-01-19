using OpenApiToCS.Generator;
using OpenApiToCS.Generator.Models;

namespace OpenApiToCS;

public static class OutputWriter
{
    public static async Task WriteGeneratedFiles(string outputDirectory, DataClassGenerationResult dataClasses, Dictionary<string, string> apiClasses, Dictionary<string, string>? webhooks = null)
    {
        Directory.CreateDirectory(Path.Combine(outputDirectory, "Models"));
        Directory.CreateDirectory(Path.Combine(outputDirectory, "Api"));
        
        if (webhooks != null && webhooks.Count > 0)
        {
            Directory.CreateDirectory(Path.Combine(outputDirectory, "Webhooks"));
        }

        await WriteDataClasses(outputDirectory, dataClasses);
        await WriteApiClients(outputDirectory, apiClasses);
        
        if (webhooks != null)
        {
            await WriteWebhooks(outputDirectory, webhooks);
        }
    }

    private static async Task WriteDataClasses(string outputDirectory, DataClassGenerationResult dataClasses)
    {
        var usedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var dataClass in dataClasses.Classes)
        {
            var fileName = GetSafeFileName(dataClass.Key, usedFileNames);
            await File.WriteAllTextAsync(
                Path.Combine(outputDirectory, "Models", fileName),
                dataClass.Value.Source);

            await WriteOneOfConverters(outputDirectory, dataClass.Value.OneOfConverters);
        }
    }

    private static async Task WriteOneOfConverters(string outputDirectory, List<OneOfConverter> converters)
    {
        foreach (var converter in converters)
        {
            await File.WriteAllTextAsync(
                Path.Combine(outputDirectory, "Models", $"{converter.Name}.cs"),
                converter.Source);

            foreach (var oneOf in converter.OneOfs)
            {
                await File.WriteAllTextAsync(
                    Path.Combine(outputDirectory, "Models", $"{oneOf.Name}.cs"),
                    oneOf.Source);
            }
        }
    }

    private static async Task WriteApiClients(string outputDirectory, Dictionary<string, string> apiClasses)
    {
        foreach (var apiClass in apiClasses)
        {
            await File.WriteAllTextAsync(
                Path.Combine(outputDirectory, "Api", $"{apiClass.Key}.cs"),
                apiClass.Value);
        }
    }
    
    private static async Task WriteWebhooks(string outputDirectory, Dictionary<string, string> webhooks)
    {
        foreach (var webhook in webhooks)
        {
            await File.WriteAllTextAsync(
                Path.Combine(outputDirectory, "Webhooks", $"{webhook.Key}.cs"),
                webhook.Value);
        }
    }

    private static string GetSafeFileName(string className, HashSet<string> usedFileNames)
    {
        string baseFileName = $"{className}.cs";
        
        // If no collision, use the base name
        if (usedFileNames.Add(baseFileName))
        {
            return baseFileName;
        }
        
        // On Windows (case-insensitive file system), if there's a collision, append index
        if (Environment.OSVersion.Platform is PlatformID.Win32NT or PlatformID.Win32Windows or PlatformID.Win32S)
        {
            int index = 1;
            string indexedFileName;
            do
            {
                indexedFileName = $"{className}{index}.cs";
                index++;
            } while (!usedFileNames.Add(indexedFileName));
            
            return indexedFileName;
        }
        
        // On Unix-like systems, case is significant, so baseFileName should work
        return baseFileName;
    }
}
