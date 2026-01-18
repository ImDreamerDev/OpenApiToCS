using OpenApiToCS.Generator;
using OpenApiToCS.Generator.Models;

namespace OpenApiToCS;

public static class OutputWriter
{
    public static async Task WriteGeneratedFiles(string outputDirectory, DataClassGenerationResult dataClasses, Dictionary<string, string> apiClasses)
    {
        Directory.CreateDirectory(Path.Combine(outputDirectory, "Models"));
        Directory.CreateDirectory(Path.Combine(outputDirectory, "Api"));

        await WriteDataClasses(outputDirectory, dataClasses);
        await WriteApiClients(outputDirectory, apiClasses);
    }

    private static async Task WriteDataClasses(string outputDirectory, DataClassGenerationResult dataClasses)
    {
        int index = 0;
        foreach (var dataClass in dataClasses.Classes)
        {
            var fileName = GetSafeFileName(dataClass.Key, index);
            await File.WriteAllTextAsync(
                Path.Combine(outputDirectory, "Models", fileName),
                dataClass.Value.Source);

            await WriteOneOfConverters(outputDirectory, dataClass.Value.OneOfConverters);
            index++;
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

    private static string GetSafeFileName(string className, int index)
    {
        // Append index on Windows to avoid case-sensitivity issues
        return Environment.OSVersion.Platform is PlatformID.Win32NT or PlatformID.Win32Windows or PlatformID.Win32S
            ? $"{className}{index}.cs"
            : $"{className}.cs";
    }
}
