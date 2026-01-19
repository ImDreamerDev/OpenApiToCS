using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;
using Xunit;

namespace OpenApiToCS.Tests;

public class MockServerGeneratorTests
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
    public void Should_Generate_Mock_Server()
    {
        // Arrange
        var document = LoadDocument("TestData/petstore.json");
        var mockGenerator = new MockServerGenerator(document);
        var outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Act
            mockGenerator.Generate(outputPath);

            // Assert
            Directory.Exists(outputPath).ShouldBeTrue();
            File.Exists(Path.Combine(outputPath, "MockServer.csproj")).ShouldBeTrue();
            File.Exists(Path.Combine(outputPath, "Program.cs")).ShouldBeTrue();
            File.Exists(Path.Combine(outputPath, "README.md")).ShouldBeTrue();
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
        }
    }

    [Fact]
    public void Should_Generate_Program_With_Endpoints()
    {
        // Arrange
        var document = LoadDocument("TestData/petstore.json");
        var mockGenerator = new MockServerGenerator(document);
        var outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Act
            mockGenerator.Generate(outputPath);
            var programCs = File.ReadAllText(Path.Combine(outputPath, "Program.cs"));

            // Assert
            programCs.ShouldContain("app.MapGET");
            programCs.ShouldContain("app.MapPOST");
            programCs.ShouldContain("app.MapDELETE");
            programCs.ShouldContain("/pets");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
        }
    }

    [Fact]
    public void Should_Generate_Program_With_CORS()
    {
        // Arrange
        var document = LoadDocument("TestData/petstore.json");
        var mockGenerator = new MockServerGenerator(document);
        var outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Act
            mockGenerator.Generate(outputPath);
            var programCs = File.ReadAllText(Path.Combine(outputPath, "Program.cs"));

            // Assert
            programCs.ShouldContain("AddCors");
            programCs.ShouldContain("UseCors");
            programCs.ShouldContain("AllowAnyOrigin");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
        }
    }

    [Fact]
    public void Should_Generate_README_With_Endpoints()
    {
        // Arrange
        var document = LoadDocument("TestData/petstore.json");
        var mockGenerator = new MockServerGenerator(document);
        var outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Act
            mockGenerator.Generate(outputPath);
            var readme = File.ReadAllText(Path.Combine(outputPath, "README.md"));

            // Assert
            readme.ShouldContain("Mock Server");
            readme.ShouldContain("Pet Store API");
            readme.ShouldContain("GET /pets");
            readme.ShouldContain("POST /pets");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
        }
    }

    [Fact]
    public void Should_Generate_Valid_Project_File()
    {
        // Arrange
        var document = LoadDocument("TestData/petstore.json");
        var mockGenerator = new MockServerGenerator(document);
        var outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Act
            mockGenerator.Generate(outputPath);
            var csproj = File.ReadAllText(Path.Combine(outputPath, "MockServer.csproj"));

            // Assert
            csproj.ShouldContain("<Project Sdk=\"Microsoft.NET.Sdk.Web\">");
            csproj.ShouldContain("net10.0");
            csproj.ShouldContain("<Nullable>enable</Nullable>");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
        }
    }
}
