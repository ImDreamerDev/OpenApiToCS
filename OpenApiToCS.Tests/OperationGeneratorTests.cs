using System.Net;
using OpenApiToCS.OpenApi;
using Shouldly;

namespace OpenApiToCS.Generator;

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

        var result = OperationGenerator.GenerateApiClasses(doc);

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

        var result = OperationGenerator.GenerateApiClasses(doc);

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

        var result = OperationGenerator.GenerateApiClasses(doc);

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

        var result = OperationGenerator.GenerateApiClasses(doc);

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
}