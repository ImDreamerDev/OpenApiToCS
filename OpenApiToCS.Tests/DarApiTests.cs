using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;

namespace OpenApiToCS.Tests;

public class DarApiTests
{
    private readonly OpenApiDocument _document;

    public DarApiTests()
    {
        string json = File.ReadAllText("TestData/DAR.json");
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        _document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;
    }

    [Fact]
    public void Should_Deserialize_DAR_OpenApi_Document()
    {
        _document.ShouldNotBeNull();
        _document.OpenApiVersion.ShouldBe("3.0.1");
        _document.Info.Title.ShouldBe("Datafordeleren Haendelser API");
        _document.Info.Version.ShouldBe("1.0");
    }

    [Fact]
    public void Should_Generate_EventWrapper_Model_With_Required_Properties()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("EventWrapper");
        var eventWrapperClass = result.Classes["EventWrapper"];
        
        eventWrapperClass.Source.ShouldContain("public record EventWrapper");
        eventWrapperClass.Source.ShouldContain("public float Id");
        eventWrapperClass.Source.ShouldContain("public MessageType Message");
        eventWrapperClass.Source.ShouldContain("public string Format");
        eventWrapperClass.Source.ShouldContain("public DateTimeOffset Timestamp");
        
        // Check required properties
        eventWrapperClass.Properties.First(p => p.Name == "Id").IsRequired.ShouldBeTrue();
        eventWrapperClass.Properties.First(p => p.Name == "Message").IsRequired.ShouldBeTrue();
        eventWrapperClass.Properties.First(p => p.Name == "Format").IsRequired.ShouldBeTrue();
        eventWrapperClass.Properties.First(p => p.Name == "Timestamp").IsRequired.ShouldBeTrue();
    }

    [Fact]
    public void Should_Generate_MessageType_Model()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("MessageType");
        var messageTypeClass = result.Classes["MessageType"];
        
        messageTypeClass.Source.ShouldContain("public record MessageType");
    }

    [Fact]
    public void Should_Generate_CustomClient()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("CustomClientV1");
        var clientSource = result["CustomClientV1"];
        
        clientSource.ShouldContain("public class CustomClientV1");
        clientSource.ShouldContain("HttpClient httpClient");
    }

    [Fact]
    public void Should_Generate_GetCustom_Method()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        var clientSource = result["CustomClientV1"];
        
        clientSource.ShouldContain("public async Task");
        clientSource.ShouldContain("GetCustom");
    }

    [Fact]
    public void Should_Use_Correct_Namespace()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        foreach (var apiClass in result.Values)
        {
            apiClass.ShouldContain("namespace DatafordelerenHaendelserAPIApiClientV1;");
            apiClass.ShouldContain("using DatafordelerenHaendelserAPIApiClientV1.Models;");
        }
    }

    [Fact]
    public void Should_Generate_Data_Models_With_Correct_Namespace()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        foreach (var dataClass in result.Classes.Values)
        {
            dataClass.Namespace.ShouldBe("DatafordelerenHaendelserAPIApiClientV1.Models");
            dataClass.Source.ShouldContain("namespace DatafordelerenHaendelserAPIApiClientV1.Models;");
        }
    }

    [Fact]
    public void Should_Handle_DateTime_Format_Correctly()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        var eventWrapperClass = result.Classes["EventWrapper"];
        
        // Timestamp should be DateTimeOffset for date-time format
        eventWrapperClass.Source.ShouldContain("public DateTimeOffset Timestamp");
    }

    [Fact]
    public void Should_Handle_Number_Type_For_Id()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        var eventWrapperClass = result.Classes["EventWrapper"];
        
        // OpenAPI "number" type should map to float
        eventWrapperClass.Source.ShouldContain("public float Id");
    }

    [Fact]
    public void Should_Generate_All_Required_Models()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("EventWrapper");
        result.Classes.Keys.ShouldContain("MessageType");
        
        // DAR has nested schemas in MessageType, so expect more than 2 models
        result.ClassCount.ShouldBeGreaterThan(2);
    }

    [Fact]
    public void Should_Generate_Required_Attribute_For_Required_Properties()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        var eventWrapperClass = result.Classes["EventWrapper"];
        
        // Required properties should have [Required] attribute
        eventWrapperClass.Source.ShouldContain("[Required]");
    }
}
