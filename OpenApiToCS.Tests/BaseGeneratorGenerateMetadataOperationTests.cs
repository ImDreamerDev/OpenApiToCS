using System.Net;
using System.Reflection;
using System.Text;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;
namespace OpenApiToCS.Tests;

public class BaseGeneratorGenerateMetadataOperationTests
{
    private static MethodInfo GetOperationMetadataMethod()
    {
        return typeof(BaseGenerator)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .First(m =>
            {
                var parameters = m.GetParameters();
                return m.Name == "GenerateMetadata"
                       && parameters.Length == 4
                       && parameters[0].ParameterType == typeof(StringBuilder)
                       && parameters[1].ParameterType == typeof(string)
                       && parameters[2].ParameterType == typeof(OpenApiOperation)
                       && parameters[3].ParameterType == typeof(int);
            });
    }

    [Fact]
    public void GenerateMetadata_Should_Emit_Operation_Metadata()
    {
        var sb = new StringBuilder();
        var operation = new OpenApiOperation
        {
            Summary = "Gets a user"
        };

        var baseGenerator = new BaseGenerator(new OpenApiDocument {OpenApiVersion = "1.0",Components = new OpenApiComponents(),Info = new OpenApiInfo
        {
            Title = "Test API",
            Version = "1.0"
        }}) { EmitMetadata = true };
        GetOperationMetadataMethod().Invoke(baseGenerator, [sb, "getUser", operation, 0]);

        var output = sb.ToString();
        output.ShouldContain("// Operation: getUser");
        output.ShouldContain("// Summary: Gets a user");
    }

    [Fact]
    public void GenerateMetadata_Should_Emit_RequestBody_Metadata()
    {
        var sb = new StringBuilder();
        var operation = new OpenApiOperation
        {
            RequestBody = new OpenApiRequestBody
            {
                Description = "User data",
                Content = new Dictionary<string, OpenApiSchemaContainer>
                {
                    ["application/json"] = new OpenApiSchemaContainer
                    {
                        Schema = new OpenApiSchema { Type = "object" }
                    }
                }
            }
        };

        var baseGenerator = new BaseGenerator(new OpenApiDocument {OpenApiVersion = "1.0",Components = new OpenApiComponents(),Info = new OpenApiInfo
        {
            Title = "Test API",
            Version = "1.0"
        }}) { EmitMetadata = true };
        GetOperationMetadataMethod().Invoke(baseGenerator, [sb, "createUser", operation, 0]);

        var output = sb.ToString();
        output.ShouldContain("// Request Body: User data");
        output.ShouldContain("// Content Type: application/json");
        output.ShouldContain("// Schema: application/json");
    }

    [Fact]
    public void GenerateMetadata_Should_Emit_Parameter_Metadata()
    {
        var sb = new StringBuilder();
        var operation = new OpenApiOperation
        {
            Parameters =
            [
                new OpenApiParameter
                {
                    Name = "id",
                    In = "path",
                    Description = "User ID",
                    Required = true,
                    Deprecated = false,
                    Schema = new OpenApiSchema { Type = "string" }
                }
            ]
        };

        var baseGenerator = new BaseGenerator(new OpenApiDocument {OpenApiVersion = "1.0",Components = new OpenApiComponents(),Info = new OpenApiInfo
        {
            Title = "Test API",
            Version = "1.0"
        }}) { EmitMetadata = true };
        GetOperationMetadataMethod().Invoke(baseGenerator, [sb, "getUser", operation, 0]);

        var output = sb.ToString();
        output.ShouldContain("// Parameters:");
        output.ShouldContain("// - id (path)");
        output.ShouldContain("//   Description: User ID");
        output.ShouldContain("//   Required: True");
        output.ShouldContain("//   Deprecated: False");
        output.ShouldContain("// Schema: id");
    }

    [Fact]
    public void GenerateMetadata_Should_Emit_Response_Metadata()
    {
        var sb = new StringBuilder();
        var operation = new OpenApiOperation
        {
            Responses = new Dictionary<HttpStatusCode, OpenApiResponse>
            {
                [HttpStatusCode.OK] = new OpenApiResponse
                {
                    Description = "Success",
                    Content = new Dictionary<string, OpenApiSchemaContainer>
                    {
                        ["application/json"] = new OpenApiSchemaContainer()
                        {
                            Schema = new OpenApiSchema { Type = "object" }
                        }
                    }
                }
            }
        };

        var baseGenerator = new BaseGenerator(new OpenApiDocument {OpenApiVersion = "1.0",Components = new OpenApiComponents(),Info = new OpenApiInfo
        {
            Title = "Test API",
            Version = "1.0"
        }}) { EmitMetadata = true };
        GetOperationMetadataMethod().Invoke(baseGenerator, [sb, "getUser", operation, 0]);

        var output = sb.ToString();
        output.ShouldContain("// Operation: getUser");
        output.ShouldContain("// Responses:");
        output.ShouldContain("// - Ok: Success");
        output.ShouldContain("//   Content Type: application/json");
        output.ShouldContain("// Schema: application/json");
    }
}