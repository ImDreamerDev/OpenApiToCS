using System.Text;
using OpenApiToCS.OpenApi;
using Shouldly;

namespace OpenApiToCS.Generator;

public class BaseGeneratorGenerateMetadataTests
{
    public BaseGeneratorGenerateMetadataTests()
    {
        // Ensure metadata emission is enabled for tests
        BaseGenerator.EmitMetadata = true;
    }

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
            .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
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
            .Invoke(null, [sb, "Id", schema, 0]);

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
            .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
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
            .Invoke(null, [sb, "Status", schema, 0]);

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
            .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
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
            .Invoke(null, [sb, "Numbers", schema, 0]);

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
            .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
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
            .Invoke(null, [sb, "RefProp", schema, 0]);

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
            .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
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
            .Invoke(null, [sb, "Person", schema, 0]);

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
            .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
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
            .Invoke(null, [sb, "Person", schema, 0]);

        var output = sb.ToString();
        output.ShouldContain("// Properties:");
        output.ShouldContain("// - Name: string");
        output.ShouldContain("// - Age: int");
        output.ShouldContain("// Schema: Name");
        output.ShouldContain("// Schema: Age");
    }
}