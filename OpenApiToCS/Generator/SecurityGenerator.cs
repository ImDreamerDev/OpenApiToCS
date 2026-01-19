using System.Text;
using OpenApiToCS.OpenApi;

namespace OpenApiToCS.Generator;

public class SecurityGenerator(OpenApiDocument document) : BaseGenerator(document)
{
    public string GenerateSecurityApplication()
    {
        if (Document.Components?.SecuritySchemes == null || Document.Components.SecuritySchemes.Count == 0)
            return string.Empty;
            
        var sb = new StringBuilder();
        foreach (var scheme in Document.Components.SecuritySchemes)
        {
            var schemeName = scheme.Key;
            var schemeValue = scheme.Value;
            var propertyName = schemeName.ToTitleCase();
            
            string headerName;
            string headerValue;
            
            switch (schemeValue.Type.ToLower())
            {
                case "http" when schemeValue.Scheme?.ToLower() == "bearer":
                    headerName = "Authorization";
                    headerValue = "$\"Bearer {_options." + propertyName + "}\"";
                    break;
                    
                case "http" when schemeValue.Scheme?.ToLower() == "basic":
                    headerName = "Authorization";
                    headerValue = "$\"Basic {_options." + propertyName + "}\"";
                    break;
                    
                case "apikey" when schemeValue.In?.ToLower() == "header":
                    headerName = schemeValue.Name ?? "X-API-Key";
                    headerValue = "_options." + propertyName;
                    break;
                    
                default:
                    continue; // Skip unsupported schemes
            }
            
            var replacements = new Dictionary<string, string>
            {
                ["schemeName"] = schemeName,
                ["schemeType"] = schemeValue.Type,
                ["propertyName"] = propertyName,
                ["headerName"] = headerName,
                ["headerValue"] = headerValue
            };
            
            sb.Append(TemplateEngine.RenderTemplate("SecuritySchemeApplication", replacements));
        }
        
        return sb.ToString();
    }
    
    public string GenerateOptionsClass(string namespaceName, string className)
    {
        if (Document.Components?.SecuritySchemes == null || Document.Components.SecuritySchemes.Count == 0)
            return string.Empty;
            
        var propertiesSb = new StringBuilder();
        
        foreach (var scheme in Document.Components.SecuritySchemes)
        {
            var schemeName = scheme.Key;
            var schemeValue = scheme.Value;
            var propertyName = schemeName.ToTitleCase();
            
            var description = schemeValue.Description ?? $"{schemeValue.Type} authentication";
            if (schemeValue.Type.ToLower() == "http" && schemeValue.Scheme != null)
            {
                description += $" ({schemeValue.Scheme})";
            }
            
            var replacements = new Dictionary<string, string>
            {
                ["propertyName"] = propertyName,
                ["description"] = description
            };
            
            propertiesSb.Append(TemplateEngine.RenderTemplate("OptionsProperty", replacements));
        }
        
        var optionsReplacements = new Dictionary<string, string>
        {
            ["namespace"] = namespaceName,
            ["clientName"] = className,
            ["optionsClassName"] = className + "Options",
            ["properties"] = propertiesSb.ToString()
        };
        
        return TemplateEngine.RenderTemplate("ClientOptions", optionsReplacements);
    }
}
