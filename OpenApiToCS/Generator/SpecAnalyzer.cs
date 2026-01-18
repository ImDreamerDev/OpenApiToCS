using OpenApiToCS.OpenApi;

namespace OpenApiToCS.Generator;

/// <summary>
/// Analyzes OpenAPI specifications and reports quality metrics
/// </summary>
public class SpecAnalyzer
{
    private readonly OpenApiDocument _document;

    public SpecAnalyzer(OpenApiDocument document)
    {
        _document = document;
    }

    public AnalysisReport Analyze()
    {
        var report = new AnalysisReport();

        AnalyzeInfo(report);
        AnalyzePaths(report);
        AnalyzeSchemas(report);
        AnalyzeSecurity(report);
        AnalyzeComplexity(report);
        GenerateScore(report);

        return report;
    }

    private void AnalyzeInfo(AnalysisReport report)
    {
        if (string.IsNullOrWhiteSpace(_document.Info?.Title))
            report.Warnings.Add("❌ Missing title (info.title)");
        else
            report.Info.Add($"✓ Title: {_document.Info.Title}");

        if (string.IsNullOrWhiteSpace(_document.Info?.Version))
            report.Warnings.Add("❌ Missing version (info.version)");
        else
            report.Info.Add($"✓ Version: {_document.Info.Version}");

        if (string.IsNullOrWhiteSpace(_document.Info?.Description))
            report.Warnings.Add("⚠ Missing description - consider adding API description");
        else
            report.Info.Add($"✓ Has description ({_document.Info.Description.Length} chars)");
    }

    private void AnalyzePaths(AnalysisReport report)
    {
        if (_document.Paths == null || _document.Paths.Count == 0)
        {
            report.Errors.Add("❌ No paths defined - spec has no endpoints");
            return;
        }

        report.Info.Add($"✓ Paths: {_document.Paths.Count} endpoints");

        int totalOperations = 0;
        int operationsWithSummary = 0;
        var httpMethods = new Dictionary<string, int>();

        foreach (var path in _document.Paths)
        {
            var operations = new[]
            {
                ("GET", path.Value.Get),
                ("POST", path.Value.Post),
                ("PUT", path.Value.Put),
                ("DELETE", path.Value.Delete),
                ("PATCH", path.Value.Patch)
            }.Where(o => o.Item2 != null);

            foreach (var (method, operation) in operations)
            {
                totalOperations++;
                
                if (!httpMethods.ContainsKey(method))
                    httpMethods[method] = 0;
                httpMethods[method]++;
                
                if (!string.IsNullOrWhiteSpace(operation!.Summary))
                    operationsWithSummary++;
            }
        }

        report.Info.Add($"✓ Operations: {totalOperations} total");
        report.Info.Add($"  Methods: {string.Join(", ", httpMethods.Select(kv => $"{kv.Key}({kv.Value})"))}");

        if (operationsWithSummary < totalOperations)
            report.Warnings.Add($"⚠ {totalOperations - operationsWithSummary}/{totalOperations} operations missing summary");
    }

    private void AnalyzeSchemas(AnalysisReport report)
    {
        if (_document.Components?.Schemas == null || _document.Components.Schemas.Count == 0)
        {
            report.Warnings.Add("⚠ No schemas defined");
            return;
        }

        var schemasCount = _document.Components.Schemas.Count;
        report.Info.Add($"✓ Schemas: {schemasCount} models");

        int schemasWithDescription = 0;
        int schemasWithExample = 0;
        int enums = 0;
        int polymorphic = 0;

        foreach (var schema in _document.Components.Schemas)
        {
            if (!string.IsNullOrWhiteSpace(schema.Value.Description))
                schemasWithDescription++;
            
            if (schema.Value.Example != null)
                schemasWithExample++;
            
            if (schema.Value.Enum != null)
                enums++;
            
            if (schema.Value.AllOf != null || schema.Value.OneOf != null || schema.Value.AnyOf != null)
                polymorphic++;
        }

        report.Info.Add($"  Enums: {enums}, Polymorphic: {polymorphic}");

        if (schemasWithDescription < schemasCount / 2)
            report.Warnings.Add($"⚠ Only {schemasWithDescription}/{schemasCount} schemas have descriptions");
        
        if (schemasWithExample == 0)
            report.Warnings.Add($"⚠ No schemas have examples - consider adding examples");
    }

    private void AnalyzeSecurity(AnalysisReport report)
    {
        if (_document.Components?.SecuritySchemes == null || _document.Components.SecuritySchemes.Count == 0)
        {
            report.Warnings.Add("⚠ No security schemes defined");
            return;
        }

        report.Info.Add($"✓ Security: {_document.Components.SecuritySchemes.Count} scheme(s)");
        foreach (var scheme in _document.Components.SecuritySchemes)
        {
            report.Info.Add($"  - {scheme.Key}: {scheme.Value.Type}");
        }
    }

    private void AnalyzeComplexity(AnalysisReport report)
    {
        if (_document.Paths == null || _document.Components?.Schemas == null)
            return;

        var pathsCount = _document.Paths.Count;
        var schemasCount = _document.Components.Schemas.Count;

        var complexity = (pathsCount, schemasCount) switch
        {
            ( < 10, < 10) => "Low",
            ( < 50, < 50) => "Medium",
            ( < 100, < 100) => "High",
            _ => "Very High"
        };

        report.Info.Add($"✓ Complexity: {complexity}");

        if (complexity == "Very High")
        {
            report.Warnings.Add("⚠ Large spec - consider splitting into multiple services");
        }
    }

    private void GenerateScore(AnalysisReport report)
    {
        int score = 100;
        score -= report.Errors.Count * 20;
        score -= report.Warnings.Count * 5;
        score = Math.Max(0, Math.Min(100, score));

        report.Score = score;
        report.Grade = score switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _ => "F"
        };
    }
}

public class AnalysisReport
{
    public List<string> Info { get; } = new();
    public List<string> Warnings { get; } = new();
    public List<string> Errors { get; } = new();
    public int Score { get; set; }
    public string Grade { get; set; } = "F";

    public void Print()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("📊 OpenAPI Specification Analysis Report");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();

        Console.WriteLine("📋 Summary:");
        foreach (var info in Info)
        {
            Console.WriteLine($"  {info}");
        }
        Console.WriteLine();

        if (Warnings.Count > 0)
        {
            Console.WriteLine($"⚠️  Warnings ({Warnings.Count}):");
            foreach (var warning in Warnings)
            {
                Console.WriteLine($"  {warning}");
            }
            Console.WriteLine();
        }

        if (Errors.Count > 0)
        {
            Console.WriteLine($"❌ Errors ({Errors.Count}):");
            foreach (var error in Errors)
            {
                Console.WriteLine($"  {error}");
            }
            Console.WriteLine();
        }

        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine($"📈 Quality Score: {Score}/100 (Grade: {Grade})");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();

        if (Score >= 80)
            Console.WriteLine("✨ Great spec quality! Ready for production.");
        else if (Score >= 60)
            Console.WriteLine("👍 Good spec, but could use some improvements.");
        else
            Console.WriteLine("⚠️  Spec needs significant improvements.");
        
        Console.WriteLine();
    }
}
