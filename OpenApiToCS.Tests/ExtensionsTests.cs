using OpenApiToCS.Generator;
using Shouldly;

namespace OpenApiToCS.Tests;

public class ExtensionsTests
{
    [Theory]
    [InlineData("test", "Test")]
    [InlineData("test_case", "TestCase")]
    [InlineData("test-case", "TestCase")]
    [InlineData("test case", "TestCase")]
    [InlineData("Test", "Test")]
    [InlineData("t", "T")]
    [InlineData("testCase", "TestCase")]
    [InlineData("test_case_example", "TestCaseExample")]
    public void ToTitleCase_Should_Convert_To_PascalCase(string input, string expected)
    {
        input.ToTitleCase().ShouldBe(expected);
    }

    [Fact]
    public void ToTitleCase_Should_Throw_On_Null()
    {
        string? input = null;
        Should.Throw<ArgumentNullException>(() => input!.ToTitleCase());
    }

    [Fact]
    public void ToTitleCase_Should_Throw_On_Empty()
    {
        Should.Throw<ArgumentException>(() => "".ToTitleCase());
    }

    [Theory]
    [InlineData("Test", "test")]
    [InlineData("T", "t")]
    [InlineData("TestCase", "testCase")]
    [InlineData("Testcase", "testcase")]
    public void FirstCharToLower_Should_Lowercase_First_Char(string input, string expected)
    {
        input.FirstCharToLower().ShouldBe(expected);
    }

    [Fact]
    public void FirstCharToLower_Should_Throw_On_Null()
    {
        string? input = null;
        Should.Throw<ArgumentNullException>(() => input!.FirstCharToLower());
    }

    [Fact]
    public void FirstCharToLower_Should_Throw_On_Empty()
    {
        Should.Throw<ArgumentException>(() => "".FirstCharToLower());
    }
}