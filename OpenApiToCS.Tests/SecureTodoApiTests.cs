using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;

namespace OpenApiToCS.Tests;

public class SecureTodoApiTests
{
    private readonly OpenApiDocument _document;

    public SecureTodoApiTests()
    {
        string json = File.ReadAllText("TestData/secure-todo-api-31.json");
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        _document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;
    }

    [Fact]
    public void Should_Deserialize_SecureTodoApi_Document()
    {
        _document.ShouldNotBeNull();
        _document.OpenApiVersion.ShouldBe("3.1.0");
        _document.Info.Title.ShouldBe("Secure Todo API");
        _document.Info.Version.ShouldBe("1.0.0");
        _document.IsOpenApi31.ShouldBeTrue();
    }

    [Fact]
    public void Should_Have_Security_Schemes()
    {
        _document.Components.SecuritySchemes.ShouldNotBeNull();
        _document.Components.SecuritySchemes.Count.ShouldBe(2);
        _document.Components.SecuritySchemes.ShouldContainKey("bearerAuth");
        _document.Components.SecuritySchemes.ShouldContainKey("apiKey");
        
        var bearerAuth = _document.Components.SecuritySchemes["bearerAuth"];
        bearerAuth.Type.ShouldBe("http");
        bearerAuth.Scheme.ShouldBe("bearer");
        bearerAuth.BearerFormat.ShouldBe("JWT");
        
        var apiKey = _document.Components.SecuritySchemes["apiKey"];
        apiKey.Type.ShouldBe("apiKey");
        apiKey.In.ShouldBe("header");
        apiKey.Name.ShouldBe("X-API-Key");
    }

    [Fact]
    public void Should_Generate_Todo_Model_With_Nullable_Description()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Todo");
        var todoClass = result.Classes["Todo"];
        
        // Check for nullable description property (OpenAPI 3.1 type array syntax)
        todoClass.Source.ShouldContain("public string? Description { get; init; }");
        
        // Check required properties
        todoClass.Properties.First(p => p.Name == "Id").IsRequired.ShouldBeTrue();
        todoClass.Properties.First(p => p.Name == "Title").IsRequired.ShouldBeTrue();
        todoClass.Properties.First(p => p.Name == "Completed").IsRequired.ShouldBeTrue();
        todoClass.Properties.First(p => p.Name == "CreatedAt").IsRequired.ShouldBeTrue();
        
        // Check optional properties
        todoClass.Properties.First(p => p.Name == "Description").IsRequired.ShouldBeFalse();
        todoClass.Properties.First(p => p.Name == "CompletedAt").IsRequired.ShouldBeFalse();
    }

    [Fact]
    public void Should_Generate_TodoPriority_Enum()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("TodoPriority");
        var priorityEnum = result.Classes["TodoPriority"];
        
        priorityEnum.Source.ShouldContain("public enum TodoPriority");
        priorityEnum.Source.ShouldContain("Low");
        priorityEnum.Source.ShouldContain("Medium");
        priorityEnum.Source.ShouldContain("High");
    }

    [Fact]
    public void Should_Generate_Options_Class_With_Both_Security_Schemes()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();
        
        // Find the options class
        var optionsKey = result.Keys.FirstOrDefault(k => k.Contains("Options"));
        optionsKey.ShouldNotBeNull();
        
        var optionsClass = result[optionsKey];
        
        // Should have both security properties
        optionsClass.ShouldContain("public string? BearerAuth { get; set; }");
        optionsClass.ShouldContain("public string? ApiKey { get; set; }");
        
        // Should have descriptions
        optionsClass.ShouldContain("JWT Bearer token authentication");
        optionsClass.ShouldContain("API Key for service-to-service authentication");
    }

    [Fact]
    public void Should_Handle_Nullable_Query_Parameter()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();
        
        var allClientCode = string.Join("\n", result.Values);
        
        // Should support nullable parameters (OpenAPI 3.1 type arrays)
        (allClientCode.Contains("bool?") || allClientCode.Contains("nullable")).ShouldBeTrue();
    }

    [Fact]
    public void Should_Have_Proper_Formatting_In_Generated_Code()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();
        
        var allClientCode = string.Join("\n", result.Values);
        
        // Check for proper indentation (tabs)
        allClientCode.ShouldContain("\t\tHttpRequestMessage httpRequest");
        
        // Check proper structure
        allClientCode.ShouldContain("using System");
    }
}
