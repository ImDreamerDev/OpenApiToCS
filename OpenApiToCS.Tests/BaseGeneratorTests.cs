using System.Reflection;
using System.Text;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;

namespace OpenApiToCS.Tests;

public class BaseGeneratorTests
{
    [Theory]
    [InlineData("MyClass", "MyClass")]
    [InlineData("components/schemas/MyClass", "MyClass")]
    [InlineData("components.schemas.MyClass", "MyClass")]
    [InlineData("my-class", "my_class")]
    [InlineData("my.class", "class")]
    [InlineData("my class", "my_class")]
    [InlineData("my{class}", "my_class_")]
    [InlineData(null, "object")]
    [InlineData("", "object")]
    public void GetClassNameFromKey_Should_Sanitize_And_Extract_Name(string? key, string expected)
    {
        var result = typeof(BaseGenerator)
            .GetMethod("GetClassNameFromKey", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(new BaseGenerator(), [key]) as string;
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("integer", null, "int")]
    [InlineData("integer", "int32", "int")]
    [InlineData("integer", "int64", "long")]
    [InlineData("number", null, "float")]
    [InlineData("number", "float", "float")]
    [InlineData("number", "double", "double")]
    [InlineData("boolean", null, "bool")]
    [InlineData("string", null, "string")]
    [InlineData("string", "date-time", "DateTimeOffset")]
    [InlineData("string", "date", "DateOnly")]
    [InlineData("string", "time", "TimeOnly")]
    [InlineData("string", "uuid", "Guid")]
    [InlineData("string", "binary", "byte[]")]
    [InlineData("string", "uri", "Uri")]
    [InlineData("array", null, "object[]")]
    [InlineData(null, null, "object")]
    public void GetTypeFromKey_Should_Map_Types(string? type, string? format, string expected)
    {
        OpenApiSchema schema = new OpenApiSchema { Type = type, Format = format };
        var result = typeof(BaseGenerator)
            .GetMethod("GetTypeFromKey", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(new BaseGenerator(), [schema, null]) as string;
        result.ShouldBe(expected);
    }

    [Fact]
    public void GetTypeFromKey_Should_Return_Array_Type()
    {
        OpenApiSchema schema = new OpenApiSchema
        {
            Type = "array",
            Items = new OpenApiSchema { Type = "string" }
        };
        var result = typeof(BaseGenerator)
            .GetMethod("GetTypeFromKey", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(new BaseGenerator(), [schema, null]) as string;
        result.ShouldBe("string[]");
    }

    [Fact]
    public void GetTypeFromKey_Should_Throw_On_Unknown_Type()
    {
        OpenApiSchema schema = new OpenApiSchema { Type = "unknown" };
        Should.Throw<TargetInvocationException>(() =>
        {
            typeof(BaseGenerator)
                .GetMethod("GetTypeFromKey", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(new BaseGenerator(), [schema, null]);
        });
    }

    [Fact]
    public void GetTypeFromKey_Should_Use_Reference()
    {
        OpenApiSchema schema = new OpenApiSchema { Reference = "#/components/schemas/RefType" };
        var result = typeof(BaseGenerator)
            .GetMethod("GetTypeFromKey", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(new BaseGenerator(), [schema, null]) as string;
        result.ShouldBe("RefType");
    }

    [Fact]
    public void GenerateSummary_Should_Generate_XmlDoc()
    {
        StringBuilder sb = new StringBuilder();
        var summary = "This is a summary.";
        StringBuilder? result = typeof(BaseGenerator)
            .GetMethod("GenerateSummary", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(new BaseGenerator(), [sb, summary]) as StringBuilder;
        var output = result!.ToString();
        output.ShouldContain("<summary>");
        output.ShouldContain("This is a summary.");
    }

    [Fact]
    public void GenerateSummary_Should_Handle_Null_Or_Empty()
    {
        StringBuilder sb = new StringBuilder();
        StringBuilder? result = typeof(BaseGenerator)
            .GetMethod("GenerateSummary", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(new BaseGenerator(), [sb, null]) as StringBuilder;
        result.ShouldBe(sb);

        sb = new StringBuilder();
        result = typeof(BaseGenerator)
            .GetMethod("GenerateSummary", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(new BaseGenerator(), [sb, ""]) as StringBuilder;
        result.ShouldBe(sb);
    }

    [Fact]
    public void GenerateSummary_Should_Append_SingleLine_Summary()
    {
        StringBuilder sb = new StringBuilder();
        StringBuilder? result = typeof(BaseGenerator)
            .GetMethod("GenerateSummary", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(new BaseGenerator(), [sb, "This is a summary."]) as StringBuilder;

        var output = result!.ToString();
        output.ShouldContain("<summary>");
        output.ShouldContain("This is a summary.");
        output.ShouldContain("</summary>");
    }

    [Fact]
    public void GenerateSummary_Should_Replace_Newlines_In_Summary()
    {
        StringBuilder sb = new StringBuilder();
        var summary = "Line1\nLine2\nLine3";
        StringBuilder? result = typeof(BaseGenerator)
            .GetMethod("GenerateSummary", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(new BaseGenerator(), [sb, summary]) as StringBuilder;

        var output = result!.ToString();
        output.ShouldContain("<summary>");
        output.ShouldContain("Line1 Line2 Line3");
        output.ShouldNotContain("\nLine2");
        output.ShouldContain("</summary>");
    }

    [Fact]
    public void GenerateSummary_Should_Return_Same_StringBuilder_On_Null_Or_Empty()
    {
        StringBuilder sb1 = new StringBuilder("start");
        StringBuilder? result1 = typeof(BaseGenerator)
            .GetMethod("GenerateSummary", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(new BaseGenerator(), [sb1, null]) as StringBuilder;
        result1.ShouldBe(sb1);
        result1!.ToString().ShouldBe("start");

        StringBuilder sb2 = new StringBuilder("start");
        StringBuilder? result2 = typeof(BaseGenerator)
            .GetMethod("GenerateSummary", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(new BaseGenerator(), [sb2, ""]) as StringBuilder;
        result2.ShouldBe(sb2);
        result2!.ToString().ShouldBe("start");
    }
}