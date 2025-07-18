using System.Reflection;
using System.Text;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;
namespace OpenApiToCS.Tests;

public class BaseGeneratorGenerateMetadataTests
{
    [Fact]
    public void GenerateMetadata_Should_Emit_Basic_Metadata()
    {
        var sb = new StringBuilder();
        var schema = new OpenApiSchema
        {
            Type = "string",
            Format = "uuid",
            Description = "A unique identifier"
        };

        typeof(BaseGenerator)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .First(m =>
            {
                var parameters = m.GetParameters();
                return m.Name == "GenerateMetadata"
                       && parameters.Length == 4
                       && parameters[0].ParameterType == typeof(StringBuilder)
                       && parameters[1].ParameterType == typeof(string)
                       && parameters[2].ParameterType == typeof(OpenApiSchema)
                       && parameters[3].ParameterType == typeof(int);
            })
            .Invoke(new BaseGenerator(new OpenApiDocument {OpenApiVersion = "1.0",Components = new OpenApiComponents(),Info = new OpenApiInfo
            {
                Title = "Test API",
                Version = "1.0"
            }}) { EmitMetadata = true }, [sb, "Id", schema, 0]);

        var output = sb.ToString();
        output.ShouldContain("// Schema: Id");
        output.ShouldContain("// Type: string");
        output.ShouldContain("// Format: uuid");
        output.ShouldContain("// Description: A unique identifier");
        output.ShouldContain("// Nullable: False");
        output.ShouldContain("// Deprecated: False");
    }

    [Fact]
    public void GenerateMetadata_Should_Emit_Enum_Values()
    {
        var sb = new StringBuilder();
        var schema = new OpenApiSchema
        {
            Type = "string",
            Enum =
            [
                "A", "B", "C"
            ]
        };

        typeof(BaseGenerator)
            .GetMethods(BindingFlags.NonPublic| BindingFlags.Instance)
            .First(m =>
            {
                var parameters = m.GetParameters();
                return m.Name == "GenerateMetadata"
                       && parameters.Length == 4
                       && parameters[0].ParameterType == typeof(StringBuilder)
                       && parameters[1].ParameterType == typeof(string)
                       && parameters[2].ParameterType == typeof(OpenApiSchema)
                       && parameters[3].ParameterType == typeof(int);
            })
            .Invoke(new BaseGenerator(new OpenApiDocument {OpenApiVersion = "1.0",Components = new OpenApiComponents(),Info = new OpenApiInfo
            {
                Title = "Test API",
                Version = "1.0"
            }}) { EmitMetadata = true }, [sb, "Status", schema, 0]);

        var output = sb.ToString();
        output.ShouldContain("// Enum values:");
        output.ShouldContain("// - A");
        output.ShouldContain("// - B");
        output.ShouldContain("// - C");
    }

    [Fact]
    public void GenerateMetadata_Should_Emit_Array_Item_Type()
    {
        var sb = new StringBuilder();
        var schema = new OpenApiSchema
        {
            Type = "array",
            Items = new OpenApiSchema { Type = "integer", Format = "int32" }
        };

        typeof(BaseGenerator)
            .GetMethods(BindingFlags.NonPublic| BindingFlags.Instance)
            .First(m =>
            {
                var parameters = m.GetParameters();
                return m.Name == "GenerateMetadata"
                       && parameters.Length == 4
                       && parameters[0].ParameterType == typeof(StringBuilder)
                       && parameters[1].ParameterType == typeof(string)
                       && parameters[2].ParameterType == typeof(OpenApiSchema)
                       && parameters[3].ParameterType == typeof(int);
            })
            .Invoke(new BaseGenerator(new OpenApiDocument {OpenApiVersion = "1.0",Components = new OpenApiComponents(),Info = new OpenApiInfo
            {
                Title = "Test API",
                Version = "1.0"
            }}) { EmitMetadata = true }, [sb, "Numbers", schema, 0]);

        var output = sb.ToString();
        output.ShouldContain("// Items type: int");
    }

    [Fact]
    public void GenerateMetadata_Should_Emit_Reference()
    {
        var sb = new StringBuilder();
        var schema = new OpenApiSchema
        {
            Reference = "#/components/schemas/OtherType"
        };

        typeof(BaseGenerator)
            .GetMethods(BindingFlags.NonPublic| BindingFlags.Instance)
            .First(m =>
            {
                var parameters = m.GetParameters();
                return m.Name == "GenerateMetadata"
                       && parameters.Length == 4
                       && parameters[0].ParameterType == typeof(StringBuilder)
                       && parameters[1].ParameterType == typeof(string)
                       && parameters[2].ParameterType == typeof(OpenApiSchema)
                       && parameters[3].ParameterType == typeof(int);
            })
            .Invoke(new BaseGenerator(new OpenApiDocument {OpenApiVersion = "1.0",Components = new OpenApiComponents(),Info = new OpenApiInfo
            {
                Title = "Test API",
                Version = "1.0"
            }}) { EmitMetadata = true }, [sb, "RefProp", schema, 0]);

        var output = sb.ToString();
        output.ShouldContain("// Reference: #/components/schemas/OtherType");
    }

    [Fact]
    public void GenerateMetadata_Should_Emit_Required_Properties()
    {
        var sb = new StringBuilder();
        var schema = new OpenApiSchema
        {
            Type = "object",
            Required =
            [
                "Name", "Age"
            ]
        };

        typeof(BaseGenerator)
            .GetMethods(BindingFlags.NonPublic| BindingFlags.Instance)
            .First(m =>
            {
                var parameters = m.GetParameters();
                return m.Name == "GenerateMetadata"
                       && parameters.Length == 4
                       && parameters[0].ParameterType == typeof(StringBuilder)
                       && parameters[1].ParameterType == typeof(string)
                       && parameters[2].ParameterType == typeof(OpenApiSchema)
                       && parameters[3].ParameterType == typeof(int);
            })
            .Invoke(new BaseGenerator(new OpenApiDocument {OpenApiVersion = "1.0",Components = new OpenApiComponents(),Info = new OpenApiInfo
            {
                Title = "Test API",
                Version = "1.0"
            }}) { EmitMetadata = true }, [sb, "Person", schema, 0]);

        var output = sb.ToString();
        output.ShouldContain("// Required properties:");
        output.ShouldContain("// - Name");
        output.ShouldContain("// - Age");
    }

    [Fact]
    public void GenerateMetadata_Should_Emit_Properties_And_Nested_Metadata()
    {
        var sb = new StringBuilder();
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["Name"] = new OpenApiSchema { Type = "string" },
                ["Age"] = new OpenApiSchema { Type = "integer", Format = "int32" }
            }
        };

        typeof(BaseGenerator)
            .GetMethods(BindingFlags.NonPublic| BindingFlags.Instance)
            .First(m =>
            {
                var parameters = m.GetParameters();
                return m.Name == "GenerateMetadata"
                       && parameters.Length == 4
                       && parameters[0].ParameterType == typeof(StringBuilder)
                       && parameters[1].ParameterType == typeof(string)
                       && parameters[2].ParameterType == typeof(OpenApiSchema)
                       && parameters[3].ParameterType == typeof(int);
            })
            .Invoke(new BaseGenerator(new OpenApiDocument {OpenApiVersion = "1.0",Components = new OpenApiComponents(),Info = new OpenApiInfo
            {
                Title = "Test API",
                Version = "1.0"
            }}) { EmitMetadata = true }, [sb, "Person", schema, 0]);

        var output = sb.ToString();
        output.ShouldContain("// Properties:");
        output.ShouldContain("// - Name: string");
        output.ShouldContain("// - Age: int");
        output.ShouldContain("// Schema: Name");
        output.ShouldContain("// Schema: Age");
    }
}