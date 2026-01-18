namespace OpenApiToCS;

public class CliOptions
{
    public string? InputFile { get; set; }
    public string OutputDirectory { get; set; } = "code";
    public string? Namespace { get; set; }
    public string? ConfigFile { get; set; }
    public string? TemplateDirectory { get; set; }
    public string? MockServerOutput { get; set; }
    public bool WatchMode { get; set; }
    public bool Verbose { get; set; }
    public bool Validate { get; set; }
    public bool Analyze { get; set; }
    public bool GenerateMockServer { get; set; }
    public bool GenerateMonoClients { get; set; }
    public bool ShowHelp { get; set; }
    public bool ShowVersion { get; set; }
}
