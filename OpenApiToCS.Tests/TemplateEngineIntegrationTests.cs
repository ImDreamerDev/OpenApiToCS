using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;

namespace OpenApiToCS.Tests;

public class TemplateEngineIntegrationTests
{
    [Theory]
    [InlineData("TestData/petstore.json", "Pet Store API", "1.0.0")]
    [InlineData("TestData/ecommerce.json", "E-Commerce API", "2.0.0")]
    [InlineData("TestData/blog.json", "Blog API", "1.5.0")]
    public void Should_Parse_And_Generate_Code_From_Different_Specs(string filePath, string expectedTitle, string expectedVersion)
    {
        // Arrange
        string json = File.ReadAllText(filePath);
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options);

        // Act & Assert - Document parsing
        document.ShouldNotBeNull();
        document.Info.Title.ShouldBe(expectedTitle);
        document.Info.Version.ShouldBe(expectedVersion);

        // Act - Code generation
        var dataGenerator = new DataClassGenerator(document);
        var dataClasses = dataGenerator.GenerateDataClasses();
        var operationGenerator = new OperationGenerator(document, dataClasses, false);
        var apiClasses = operationGenerator.GenerateApiClasses();

        // Assert - Generated classes
        dataClasses.Classes.ShouldNotBeEmpty();
        apiClasses.ShouldNotBeEmpty();

        // All generated classes should compile (contain valid C# syntax markers)
        foreach (var dataClass in dataClasses.Classes.Values)
        {
            dataClass.Source.ShouldContain("namespace ");
            dataClass.Source.ShouldContain("public ");
            
            // Should use templates (no StringBuilder artifacts)
            dataClass.Source.ShouldNotContain("sb.AppendLine");
        }

        foreach (var apiClass in apiClasses.Values)
        {
            apiClass.ShouldContain("namespace ");
            apiClass.ShouldContain("public class ");
            apiClass.ShouldContain("HttpClient httpClient");
            apiClass.ShouldContain("async Task");
            
            // Should use templates (no StringBuilder artifacts)
            apiClass.ShouldNotContain("sb.AppendLine");
        }
    }

    [Fact]
    public void All_Test_Specs_Should_Generate_Valid_Namespaces()
    {
        var testFiles = new[]
        {
            ("TestData/petstore.json", "PetStoreAPIApiClientV1"),
            ("TestData/ecommerce.json", "ECommerceAPIApiClientV2"),
            ("TestData/blog.json", "BlogAPIApiClientV1")
        };

        foreach (var (file, expectedNamespace) in testFiles)
        {
            string json = File.ReadAllText(file);
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
            };
            var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;

            var dataGenerator = new DataClassGenerator(document);
            var dataClasses = dataGenerator.GenerateDataClasses();

            // Check data class namespaces
            foreach (var dataClass in dataClasses.Classes.Values)
            {
                dataClass.Namespace.ShouldBe($"{expectedNamespace}.Models");
                dataClass.Source.ShouldContain($"namespace {expectedNamespace}.Models;");
            }

            // Check API client namespaces
            var operationGenerator = new OperationGenerator(document, dataClasses, false);
            var apiClasses = operationGenerator.GenerateApiClasses();

            foreach (var apiClass in apiClasses.Values)
            {
                apiClass.ShouldContain($"namespace {expectedNamespace};");
                apiClass.ShouldContain($"using {expectedNamespace}.Models;");
            }
        }
    }

    [Fact]
    public void All_Test_Specs_Should_Generate_Error_Handling()
    {
        var testFiles = new[]
        {
            "TestData/petstore.json",
            "TestData/ecommerce.json",
            "TestData/blog.json"
        };

        foreach (var file in testFiles)
        {
            string json = File.ReadAllText(file);
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
            };
            var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;

            var dataClasses = new DataClassGenerator(document).GenerateDataClasses();
            var apiClasses = new OperationGenerator(document, dataClasses, false).GenerateApiClasses();

            // All API clients should have error handling
            foreach (var apiClass in apiClasses.Values)
            {
                apiClass.ShouldContain("private static async Task HandleError");
                apiClass.ShouldContain("ProblemDetails");
                apiClass.ShouldContain("ProblemDetailsException");
                apiClass.ShouldContain("await HandleError(response,");
            }
        }
    }

    [Fact]
    public void All_Test_Specs_Should_Use_JsonSerializerOptions()
    {
        var testFiles = new[]
        {
            "TestData/petstore.json",
            "TestData/ecommerce.json",
            "TestData/blog.json"
        };

        foreach (var file in testFiles)
        {
            string json = File.ReadAllText(file);
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
            };
            var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;

            var dataClasses = new DataClassGenerator(document).GenerateDataClasses();
            var apiClasses = new OperationGenerator(document, dataClasses, false).GenerateApiClasses();

            // API methods with return types should accept JsonSerializerOptions
            foreach (var apiClass in apiClasses.Values)
            {
                if (apiClass.Contains("public async Task<"))
                {
                    apiClass.ShouldContain("JsonSerializerOptions? jsonSerializerOptions = null");
                    apiClass.ShouldContain("jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);");
                    apiClass.ShouldContain("jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());");
                }
            }
        }
    }

    [Fact]
    public void All_Generated_Classes_Should_Have_JsonPropertyName_Attributes()
    {
        var testFiles = new[]
        {
            "TestData/petstore.json",
            "TestData/ecommerce.json",
            "TestData/blog.json"
        };

        foreach (var file in testFiles)
        {
            string json = File.ReadAllText(file);
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
            };
            var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;

            var dataClasses = new DataClassGenerator(document).GenerateDataClasses();

            // All properties should have [JsonPropertyName] attributes
            foreach (var dataClass in dataClasses.Classes.Values.Where(c => c.Properties.Any()))
            {
                // Count JsonPropertyName occurrences should match property count
                int jsonPropertyNameCount = CountOccurrences(dataClass.Source, "[JsonPropertyName(");
                jsonPropertyNameCount.ShouldBeGreaterThanOrEqualTo(dataClass.Properties.Count);
            }
        }
    }

    [Theory]
    [InlineData("TestData/petstore.json", 3)] // Pet, NewPet, PetStatus
    [InlineData("TestData/ecommerce.json", 6)] // Product, OrderRequest, Order, OrderItem, Address, OrderStatus
    [InlineData("TestData/blog.json", 6)] // Post, PostUpdate, PostPatch, Author, Comment, PostStatus
    public void Should_Generate_Expected_Number_Of_Models(string filePath, int expectedCount)
    {
        string json = File.ReadAllText(filePath);
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;

        var dataClasses = new DataClassGenerator(document).GenerateDataClasses();

        dataClasses.ClassCount.ShouldBe(expectedCount);
    }

    private static int CountOccurrences(string source, string search)
    {
        int count = 0;
        int index = 0;
        while ((index = source.IndexOf(search, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += search.Length;
        }
        return count;
    }
}
