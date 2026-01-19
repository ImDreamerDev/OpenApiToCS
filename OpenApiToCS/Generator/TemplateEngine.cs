using System.Reflection;
using System.Text;

namespace OpenApiToCS.Generator;

public static class TemplateEngine
{
    private static readonly Dictionary<string, string> _templateCache = new();
    private static string? _customTemplateDirectory;
    
    public static void SetCustomTemplateDirectory(string? path)
    {
        _customTemplateDirectory = path;
        _templateCache.Clear(); // Clear cache when switching template directories
    }
    
    public static string RenderTemplate(string templateName, Dictionary<string, string> replacements)
    {
        string template = LoadTemplate(templateName);
        
        foreach (var replacement in replacements)
        {
            template = template.Replace($"{{{{{replacement.Key}}}}}", replacement.Value);
        }
        
        return template;
    }
    
    private static string LoadTemplate(string templateName)
    {
        string cacheKey = _customTemplateDirectory != null ? $"{_customTemplateDirectory}:{templateName}" : templateName;
        
        if (_templateCache.TryGetValue(cacheKey, out string? cached))
        {
            return cached;
        }
        
        string templatePath;
        
        // Check custom template directory first
        if (_customTemplateDirectory != null)
        {
            templatePath = Path.Combine(_customTemplateDirectory, $"{templateName}.txt");
            if (File.Exists(templatePath))
            {
                Console.WriteLine($"Using custom template: {templatePath}");
                string content = File.ReadAllText(templatePath);
                _templateCache[cacheKey] = content;
                return content;
            }
        }
        
        // Fall back to default templates
        templatePath = Path.Combine(GetTemplatesDirectory(), $"{templateName}.txt");
        
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}");
        }
        
        string defaultContent = File.ReadAllText(templatePath);
        _templateCache[cacheKey] = defaultContent;
        return defaultContent;
    }
    
    private static string GetTemplatesDirectory()
    {
        string? assemblyLocation = Assembly.GetExecutingAssembly().Location;
        string? assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        
        if (assemblyDirectory is null)
        {
            throw new InvalidOperationException("Could not determine assembly directory");
        }
        
        // Try to find Templates directory relative to assembly
        string templatesPath = Path.Combine(assemblyDirectory, "Templates");
        
        if (Directory.Exists(templatesPath))
        {
            return templatesPath;
        }
        
        // Fallback: try relative to current directory (for development)
        string currentDir = Directory.GetCurrentDirectory();
        templatesPath = Path.Combine(currentDir, "Templates");
        
        if (Directory.Exists(templatesPath))
        {
            return templatesPath;
        }
        
        // Last resort: search up the directory tree
        string? searchDir = assemblyDirectory;
        while (searchDir is not null)
        {
            templatesPath = Path.Combine(searchDir, "Templates");
            if (Directory.Exists(templatesPath))
            {
                return templatesPath;
            }
            searchDir = Directory.GetParent(searchDir)?.FullName;
        }
        
        throw new DirectoryNotFoundException("Templates directory not found");
    }
}
