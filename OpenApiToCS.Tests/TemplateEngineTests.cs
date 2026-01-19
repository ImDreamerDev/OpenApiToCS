using OpenApiToCS.Generator;
using Shouldly;
using Xunit;

namespace OpenApiToCS.Tests;

public class TemplateEngineTests
{
    [Fact]
    public void Should_Use_Default_Template_When_No_Custom_Directory()
    {
        // Arrange
        TemplateEngine.SetCustomTemplateDirectory(null);
        var replacements = new Dictionary<string, string>
        {
            ["metadata"] = "// Test metadata",
            ["className"] = "TestClass",
            ["namespace"] = "Test.Namespace",
            ["properties"] = "public string Name { get; init; }"
        };

        // Act
        var result = TemplateEngine.RenderTemplate("RecordClass", replacements);

        // Assert
        result.ShouldContain("TestClass");
        result.ShouldContain("Test.Namespace");
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Should_Use_Custom_Template_When_Directory_Set()
    {
        // Arrange
        var customTemplateDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(customTemplateDir);
        
        try
        {
            // Create custom template
            var customTemplate = "CUSTOM TEMPLATE: {{className}} in {{namespace}}";
            File.WriteAllText(Path.Combine(customTemplateDir, "RecordClass.txt"), customTemplate);

            TemplateEngine.SetCustomTemplateDirectory(customTemplateDir);
            
            var replacements = new Dictionary<string, string>
            {
                ["className"] = "CustomClass",
                ["namespace"] = "Custom.Namespace"
            };

            // Act
            var result = TemplateEngine.RenderTemplate("RecordClass", replacements);

            // Assert
            result.ShouldContain("CUSTOM TEMPLATE");
            result.ShouldContain("CustomClass");
            result.ShouldContain("Custom.Namespace");
        }
        finally
        {
            // Cleanup
            TemplateEngine.SetCustomTemplateDirectory(null);
            if (Directory.Exists(customTemplateDir))
            {
                Directory.Delete(customTemplateDir, true);
            }
        }
    }

    [Fact]
    public void Should_Fallback_To_Default_When_Custom_Template_Not_Found()
    {
        // Arrange
        var customTemplateDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(customTemplateDir);
        
        try
        {
            // Don't create the template file - it should fall back to default
            TemplateEngine.SetCustomTemplateDirectory(customTemplateDir);
            
            var replacements = new Dictionary<string, string>
            {
                ["metadata"] = "// Test",
                ["className"] = "FallbackClass",
                ["namespace"] = "Fallback.Namespace",
                ["properties"] = "public string Name { get; init; }"
            };

            // Act
            var result = TemplateEngine.RenderTemplate("RecordClass", replacements);

            // Assert - should use default template
            result.ShouldContain("FallbackClass");
            result.ShouldNotBeNullOrEmpty();
        }
        finally
        {
            // Cleanup
            TemplateEngine.SetCustomTemplateDirectory(null);
            if (Directory.Exists(customTemplateDir))
            {
                Directory.Delete(customTemplateDir, true);
            }
        }
    }

    [Fact]
    public void Should_Clear_Cache_When_Custom_Directory_Changed()
    {
        // Arrange
        var customDir1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var customDir2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(customDir1);
        Directory.CreateDirectory(customDir2);
        
        try
        {
            File.WriteAllText(Path.Combine(customDir1, "RecordClass.txt"), "TEMPLATE1: {{className}}");
            File.WriteAllText(Path.Combine(customDir2, "RecordClass.txt"), "TEMPLATE2: {{className}}");

            var replacements = new Dictionary<string, string> { ["className"] = "Test" };

            // Act
            TemplateEngine.SetCustomTemplateDirectory(customDir1);
            var result1 = TemplateEngine.RenderTemplate("RecordClass", replacements);

            TemplateEngine.SetCustomTemplateDirectory(customDir2);
            var result2 = TemplateEngine.RenderTemplate("RecordClass", replacements);

            // Assert
            result1.ShouldContain("TEMPLATE1");
            result2.ShouldContain("TEMPLATE2");
        }
        finally
        {
            // Cleanup
            TemplateEngine.SetCustomTemplateDirectory(null);
            if (Directory.Exists(customDir1)) Directory.Delete(customDir1, true);
            if (Directory.Exists(customDir2)) Directory.Delete(customDir2, true);
        }
    }
}
