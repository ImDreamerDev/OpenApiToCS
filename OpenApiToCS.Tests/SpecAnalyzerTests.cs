using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;
using Xunit;

namespace OpenApiToCS.Tests;

public class SpecAnalyzerTests
{
    private static OpenApiDocument LoadDocument(string path)
    {
        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        return JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;
    }

    [Fact]
    public void Should_Analyze_Complete_Spec()
    {
        // Arrange
        var document = LoadDocument("TestData/petstore.json");
        var analyzer = new SpecAnalyzer(document);

        // Act
        var report = analyzer.Analyze();

        // Assert
        report.ShouldNotBeNull();
        report.Score.ShouldBeGreaterThan(0);
        report.Grade.ShouldNotBeNullOrEmpty();
        report.Info.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Should_Give_High_Score_For_Good_Spec()
    {
        // Arrange
        var document = LoadDocument("TestData/petstore.json");
        var analyzer = new SpecAnalyzer(document);

        // Act
        var report = analyzer.Analyze();

        // Assert
        report.Score.ShouldBeGreaterThanOrEqualTo(70);
        report.Grade.ShouldBeOneOf("A", "B", "C");
    }

    [Fact]
    public void Should_Detect_Missing_Title()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            OpenApiVersion = "3.0.0",
            Info = new OpenApiInfo { Title = "", Version = "1.0.0" },
            Components = new OpenApiComponents { Schemas = new Dictionary<string, OpenApiSchema>() },
            Paths = new Dictionary<string, OpenApiPath>()
        };
        var analyzer = new SpecAnalyzer(document);

        // Act
        var report = analyzer.Analyze();

        // Assert
        report.Warnings.ShouldContain(w => w.Contains("title"));
    }

    [Fact]
    public void Should_Count_Endpoints_Correctly()
    {
        // Arrange
        var document = LoadDocument("TestData/petstore.json");
        var analyzer = new SpecAnalyzer(document);

        // Act
        var report = analyzer.Analyze();

        // Assert
        report.Info.ShouldContain(i => i.Contains("Paths:"));
        report.Info.ShouldContain(i => i.Contains("Operations:"));
    }

    [Fact]
    public void Should_Count_Schemas_Correctly()
    {
        // Arrange
        var document = LoadDocument("TestData/petstore.json");
        var analyzer = new SpecAnalyzer(document);

        // Act
        var report = analyzer.Analyze();

        // Assert
        report.Info.ShouldContain(i => i.Contains("Schemas:"));
        report.Info.ShouldContain(i => i.Contains("models"));
    }

    [Fact]
    public void Should_Assess_Complexity()
    {
        // Arrange
        var document = LoadDocument("TestData/petstore.json");
        var analyzer = new SpecAnalyzer(document);

        // Act
        var report = analyzer.Analyze();

        // Assert
        report.Info.ShouldContain(i => i.Contains("Complexity:"));
    }
}
