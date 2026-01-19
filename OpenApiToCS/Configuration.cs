namespace OpenApiToCS;

public class Configuration
{
    public string? OutputDirectory { get; set; }
    public string? Namespace { get; set; }
    public string? TemplateDirectory { get; set; }
    public bool GenerateMonoClient { get; set; }
    
    // NuGet Package Configuration
    public string? PackageId { get; set; }
    public string? PackageVersion { get; set; }
    public string? PackageAuthors { get; set; }
    public string? PackageCompany { get; set; }
    public string? PackageDescription { get; set; }
    public string? PackageTags { get; set; }
    public string? PackageRepositoryUrl { get; set; }
    public string? PackageLicense { get; set; }
}
