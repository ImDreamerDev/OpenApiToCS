using System.Net;
using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.Generator.Models;
using OpenApiToCS.OpenApi;
using Shouldly;
namespace OpenApiToCS.Tests;

public class OperationGeneratorTests
{
    [Fact]
    public void GenerateApiClasses_Should_Group_By_First_Path_Segment()
    {
        var doc = new OpenApiDocument
        {
            OpenApiVersion = "3.0.0",
            Info = new OpenApiInfo { Title = "TestApi", Version = "1.0" },
            Paths = new Dictionary<string, OpenApiPath>
            {
                ["/users/get"] = new OpenApiPath { Get = new OpenApiOperation { Summary = "Get users", Responses = new Dictionary<HttpStatusCode, OpenApiResponse> { [HttpStatusCode.OK] = new OpenApiResponse { Content = new Dictionary<string, OpenApiSchemaContainer> { ["application/json"] = new OpenApiSchemaContainer { Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "string" } } } } } } } },
                ["/orders/create"] = new OpenApiPath { Post = new OpenApiOperation { Summary = "Create order", Responses = new Dictionary<HttpStatusCode, OpenApiResponse> { [HttpStatusCode.OK] = new OpenApiResponse { Content = new Dictionary<string, OpenApiSchemaContainer> { ["application/json"] = new OpenApiSchemaContainer { Schema = new OpenApiSchema { Type = "string" } } } } } } }
            },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["string"] = new OpenApiSchema { Type = "string" },
                    ["array"] = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "string" } }
                }
            }
        };

        var dataClasses = new DataClassGenerator().GenerateDataClasses(doc);
        var result = new OperationGenerator(dataClasses).GenerateApiClasses(doc);

        result.Keys.ShouldContain("UsersClientV1");
        result.Keys.ShouldContain("OrdersClientV1");
        result["UsersClientV1"].ShouldContain("public async Task<string[]> GetGet(");
        result["OrdersClientV1"].ShouldContain("public async Task<string> PostCreate(");
    }

    [Fact]
    public void GenerateApiClasses_Should_Use_Title_And_Version_For_Namespace()
    {
        var doc = new OpenApiDocument
        {
            OpenApiVersion = "3.0.0",
            Info = new OpenApiInfo { Title = "my service", Version = "2.1" },
            Paths = new Dictionary<string, OpenApiPath>
            {
                ["/foo/bar"] = new OpenApiPath { Get = new OpenApiOperation { Summary = "Get bar", Responses = new Dictionary<HttpStatusCode, OpenApiResponse> { [HttpStatusCode.OK] = new OpenApiResponse { Content = new Dictionary<string, OpenApiSchemaContainer> { ["application/json"] = new OpenApiSchemaContainer { Schema = new OpenApiSchema { Type = "string" } } } } } } }
            },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["string"] = new OpenApiSchema { Type = "string" }
                }
            }
        };

        var dataClasses = new DataClassGenerator().GenerateDataClasses(doc);
        var result = new OperationGenerator(dataClasses).GenerateApiClasses(doc);

        result.Keys.ShouldContain("FooClientV2");
        result["FooClientV2"].ShouldContain("namespace MyServiceApiClientV2;");
    }

    [Fact]
    public void GenerateApiClasses_Should_Skip_Operation_If_No_Success_Response()
    {
        var doc = new OpenApiDocument
        {
            OpenApiVersion = "3.0.0",
            Info = new OpenApiInfo { Title = "Test", Version = "1.0" },
            Paths = new Dictionary<string, OpenApiPath>
            {
                ["/no/success"] = new OpenApiPath { Get = new OpenApiOperation { Summary = "No success", Responses = new Dictionary<HttpStatusCode, OpenApiResponse>() } }
            },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["string"] = new OpenApiSchema { Type = "string" }
                }
            }
        };

        var dataClasses = new DataClassGenerator().GenerateDataClasses(doc);
        var result = new OperationGenerator(dataClasses).GenerateApiClasses(doc);

        result["NoClientV1"].ShouldNotContain("public async Task");
    }

    [Fact]
    public void GenerateApiClasses_Should_Handle_All_Http_Methods()
    {
        var doc = new OpenApiDocument
        {
            OpenApiVersion = "3.0.0",
            Info = new OpenApiInfo { Title = "Test", Version = "1.0" },
            Paths = new Dictionary<string, OpenApiPath>
            {
                ["/all/methods"] = new OpenApiPath
                {
                    Get = new OpenApiOperation { Summary = "Get", Responses = new Dictionary<HttpStatusCode, OpenApiResponse> { [HttpStatusCode.OK] = new OpenApiResponse { Content = new Dictionary<string, OpenApiSchemaContainer> { ["application/json"] = new OpenApiSchemaContainer { Schema = new OpenApiSchema { Type = "string" } } } } } },
                    Post = new OpenApiOperation { Summary = "Post", Responses = new Dictionary<HttpStatusCode, OpenApiResponse> { [HttpStatusCode.OK] = new OpenApiResponse { Content = new Dictionary<string, OpenApiSchemaContainer> { ["application/json"] = new OpenApiSchemaContainer { Schema = new OpenApiSchema { Type = "string" } } } } } },
                    Put = new OpenApiOperation { Summary = "Put", Responses = new Dictionary<HttpStatusCode, OpenApiResponse> { [HttpStatusCode.OK] = new OpenApiResponse { Content = new Dictionary<string, OpenApiSchemaContainer> { ["application/json"] = new OpenApiSchemaContainer { Schema = new OpenApiSchema { Type = "string" } } } } } },
                    Delete = new OpenApiOperation { Summary = "Delete", Responses = new Dictionary<HttpStatusCode, OpenApiResponse> { [HttpStatusCode.OK] = new OpenApiResponse { Content = new Dictionary<string, OpenApiSchemaContainer> { ["application/json"] = new OpenApiSchemaContainer { Schema = new OpenApiSchema { Type = "string" } } } } } },
                    Patch = new OpenApiOperation { Summary = "Patch", Responses = new Dictionary<HttpStatusCode, OpenApiResponse> { [HttpStatusCode.OK] = new OpenApiResponse { Content = new Dictionary<string, OpenApiSchemaContainer> { ["application/json"] = new OpenApiSchemaContainer { Schema = new OpenApiSchema { Type = "string" } } } } } },
                    Head = new OpenApiOperation { Summary = "Head", Responses = new Dictionary<HttpStatusCode, OpenApiResponse> { [HttpStatusCode.OK] = new OpenApiResponse { Content = new Dictionary<string, OpenApiSchemaContainer> { ["application/json"] = new OpenApiSchemaContainer { Schema = new OpenApiSchema { Type = "string" } } } } } },
                    Options = new OpenApiOperation { Summary = "Options", Responses = new Dictionary<HttpStatusCode, OpenApiResponse> { [HttpStatusCode.OK] = new OpenApiResponse { Content = new Dictionary<string, OpenApiSchemaContainer> { ["application/json"] = new OpenApiSchemaContainer { Schema = new OpenApiSchema { Type = "string" } } } } } },
                    Trace = new OpenApiOperation { Summary = "Trace", Responses = new Dictionary<HttpStatusCode, OpenApiResponse> { [HttpStatusCode.OK] = new OpenApiResponse { Content = new Dictionary<string, OpenApiSchemaContainer> { ["application/json"] = new OpenApiSchemaContainer { Schema = new OpenApiSchema { Type = "string" } } } } } }
                }
            },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["string"] = new OpenApiSchema { Type = "string" }
                }
            }
        };

        var dataClasses = new DataClassGenerator().GenerateDataClasses(doc);
        var result = new OperationGenerator(dataClasses).GenerateApiClasses(doc);

        var code = result["AllClientV1"];
        code.ShouldContain("public async Task<string> GetMethods(");
        code.ShouldContain("public async Task<string> PostMethods(");
        code.ShouldContain("public async Task<string> PutMethods(");
        code.ShouldContain("public async Task<string> DeleteMethods(");
        code.ShouldContain("public async Task<string> PatchMethods(");
        code.ShouldContain("public async Task<string> HeadMethods(");
        code.ShouldContain("public async Task<string> OptionsMethods(");
        code.ShouldContain("public async Task<string> TraceMethods(");
    }

    [Fact]
    public void GenerateMonoApiClass_Should_Create_Class_With_Correct_Namespace_And_ClassName()
    {
        var document = new OpenApiDocument
        {
            OpenApiVersion = "3.0.0",
            Info = new OpenApiInfo { Title = "TestApi", Version = "1.0" },
            Paths = new Dictionary<string, OpenApiPath>(),
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["string"] = new OpenApiSchema { Type = "string" }
                }
            }
        };

        var generator = new OperationGenerator(new DataClassGenerationResult(), true);
        var result = generator.GenerateApiClasses(document);

        result.Keys.ShouldContain("TestApiClientV1");
        result["TestApiClientV1"].ShouldContain("namespace TestApiApiClientV1;");
        result["TestApiClientV1"].ShouldContain("public class TestApiClientV1(HttpClient httpClient)");
    }

    [Fact]
    public void GenerateMonoApiClass_Should_Handle_Empty_Paths()
    {
        var document = new OpenApiDocument
        {
            OpenApiVersion = "3.0.0",
            Info = new OpenApiInfo { Title = "EmptyApi", Version = "1.0" },
            Paths = new Dictionary<string, OpenApiPath>(),
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["string"] = new OpenApiSchema { Type = "string" }
                }
            }
        };

        var generator = new OperationGenerator(new DataClassGenerationResult(), true);
        var result = generator.GenerateApiClasses(document);

        result.Keys.ShouldContain("EmptyApiClientV1");
        result["EmptyApiClientV1"].ShouldNotContain("public async Task");
    }

    [Fact]
    public void GenerateMonoApiClass_Should_Skip_Operations_Without_Successful_Responses()
    {
        var document = new OpenApiDocument
        {
            OpenApiVersion = "3.0.0",
            Info = new OpenApiInfo { Title = "NoSuccessApi", Version = "1.0" },
            Paths = new Dictionary<string, OpenApiPath>
            {
                ["/no-success"] = new OpenApiPath
                {
                    Get = new OpenApiOperation { Responses = new Dictionary<HttpStatusCode, OpenApiResponse>() }
                }
            },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["string"] = new OpenApiSchema { Type = "string" }
                }
            }
        };

        var generator = new OperationGenerator(new DataClassGenerationResult(), true);
        var result = generator.GenerateApiClasses(document);

        result["NoSuccessApiClientV1"].ShouldNotContain("public async Task GetNoSuccess(");
    }

    [Fact]
    public void GenerateMonoApiClass_Should_Handle_Nullable_Response_Types()
    {
        var document = new OpenApiDocument
        {
            OpenApiVersion = "3.0.0",
            Info = new OpenApiInfo { Title = "NullableApi", Version = "1.0" },
            Paths = new Dictionary<string, OpenApiPath>
            {
                ["/nullable"] = new OpenApiPath
                {
                    Get = new OpenApiOperation
                    {
                        Responses = new Dictionary<HttpStatusCode, OpenApiResponse>
                        {
                            [HttpStatusCode.OK] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiSchemaContainer>
                                {
                                    ["application/json"] = new OpenApiSchemaContainer
                                    {
                                        Schema = new OpenApiSchema { Type = "string", Nullable = true }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["string"] = new OpenApiSchema { Type = "string" }
                }
            }
        };

        var generator = new OperationGenerator(new DataClassGenerationResult(), true);
        var result = generator.GenerateApiClasses(document);

        result["NullableApiClientV1"].ShouldContain("public async Task<string?> GetNullable(");
    }

    private static OpenApiDocument LoadDarDocument()
    {
        var json = File.ReadAllText("TestData/DAR.json");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;
    }

    [Fact]
    public void GenerateApiClasses_Should_Produce_Expected_Client_And_Methods_For_DarApi()
    {
        var doc = LoadDarDocument();
        var dataClasses = new DataClassGenerator().GenerateDataClasses(doc);
        var result = new OperationGenerator(dataClasses, true).GenerateApiClasses(doc);

        // Check expected client class name
        result.Keys.ShouldContain("DatafordelerenHaendelserAPIClientV1");

        // Check representative method for /custom GET
        var customClient = result["DatafordelerenHaendelserAPIClientV1"];
        customClient.ShouldContain("namespace DatafordelerenHaendelserAPIApiClientV1;");
        customClient.ShouldContain("public class DatafordelerenHaendelserAPIClientV1(HttpClient httpClient)");
        customClient.ShouldContain("public async Task<EventWrapper[]> GetCustom(");
        customClient.ShouldContain("HttpClient httpClient");
    }
}