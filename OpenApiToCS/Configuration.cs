namespace OpenApiToCS;

public class Configuration
{
    public string? OutputDirectory { get; set; }
    public string? Namespace { get; set; }
    public string? TemplateDirectory { get; set; }
    public bool GenerateMonoClient { get; set; }
}
