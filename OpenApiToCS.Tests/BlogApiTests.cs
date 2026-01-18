using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;

namespace OpenApiToCS.Tests;

public class BlogApiTests
{
    private readonly OpenApiDocument _document;

    public BlogApiTests()
    {
        string json = File.ReadAllText("TestData/blog.json");
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        _document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;
    }

    [Fact]
    public void Should_Deserialize_Blog_OpenApi_Document()
    {
        _document.ShouldNotBeNull();
        _document.OpenApiVersion.ShouldBe("3.0.0");
        _document.Info.Title.ShouldBe("Blog API");
        _document.Info.Version.ShouldBe("1.5.0");
    }

    [Fact]
    public void Should_Generate_Post_Model_With_Author_Reference()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Post");
        var postClass = result.Classes["Post"];
        
        postClass.Source.ShouldContain("public record Post");
        postClass.Source.ShouldContain("public long Id");
        postClass.Source.ShouldContain("public string Title");
        postClass.Source.ShouldContain("public string Content");
        postClass.Source.ShouldContain("public Author Author");
        postClass.Source.ShouldContain("public string[] Tags");
        postClass.Source.ShouldContain("public DateTimeOffset PublishedAt");
        postClass.Source.ShouldContain("public PostStatus Status");
    }

    [Fact]
    public void Should_Generate_Author_Model()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Author");
        var authorClass = result.Classes["Author"];
        
        authorClass.Source.ShouldContain("public record Author");
        authorClass.Source.ShouldContain("public long Id");
        authorClass.Source.ShouldContain("public string Name");
        authorClass.Source.ShouldContain("public string Email");
        authorClass.Source.ShouldContain("public string? Bio");
        
        authorClass.Properties.First(p => p.Name == "Bio").IsNullable.ShouldBeTrue();
    }

    [Fact]
    public void Should_Generate_Comment_Model()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Comment");
        var commentClass = result.Classes["Comment"];
        
        commentClass.Source.ShouldContain("public record Comment");
        commentClass.Source.ShouldContain("public long Id");
        commentClass.Source.ShouldContain("public long PostId");
        commentClass.Source.ShouldContain("public string Author");
        commentClass.Source.ShouldContain("public string Content");
        commentClass.Source.ShouldContain("public DateTimeOffset CreatedAt");
    }

    [Fact]
    public void Should_Generate_PostUpdate_Model()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("PostUpdate");
        var updateClass = result.Classes["PostUpdate"];
        
        updateClass.Source.ShouldContain("public record PostUpdate");
        updateClass.Source.ShouldContain("public string Title");
        updateClass.Source.ShouldContain("public string Content");
        updateClass.Source.ShouldContain("public string[] Tags");
    }

    [Fact]
    public void Should_Generate_PostPatch_Model_With_Optional_Properties()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("PostPatch");
        var patchClass = result.Classes["PostPatch"];
        
        patchClass.Source.ShouldContain("public record PostPatch");
        patchClass.Source.ShouldContain("public string Title");
        patchClass.Source.ShouldContain("public string Content");
        patchClass.Source.ShouldContain("public PostStatus Status");
        
        // All properties should be optional (not required)
        patchClass.Properties.ShouldAllBe(p => p.IsRequired == false);
    }

    [Fact]
    public void Should_Generate_PostStatus_Enum()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("PostStatus");
        var enumClass = result.Classes["PostStatus"];
        
        enumClass.Source.ShouldContain("public enum PostStatus");
        enumClass.Source.ShouldContain("draft,");
        enumClass.Source.ShouldContain("published,");
        enumClass.Source.ShouldContain("archived,");
    }

    [Fact]
    public void Should_Generate_PostsClient_With_Get_Method()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("PostsClientV1");
        var clientSource = result["PostsClientV1"];
        
        clientSource.ShouldContain("public async Task<Post> GetPostId(");
        clientSource.ShouldContain("long postId");
    }

    [Fact]
    public void Should_Generate_PostsClient_With_Put_Method()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        var clientSource = result["PostsClientV1"];
        
        clientSource.ShouldContain("public async Task<Post> PutPostId(");
        clientSource.ShouldContain("long postId");
        clientSource.ShouldContain("PostUpdate postUpdate");
    }

    [Fact]
    public void Should_Generate_PostsClient_With_Patch_Method()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        var clientSource = result["PostsClientV1"];
        
        clientSource.ShouldContain("public async Task<Post> PatchPostId(");
        clientSource.ShouldContain("long postId");
        clientSource.ShouldContain("PostPatch postPatch");
    }

    [Fact]
    public void Should_Generate_PostsClient_With_GetComments_Method()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        var clientSource = result["PostsClientV1"];
        
        clientSource.ShouldContain("public async Task<Comment[]> GetComments(");
        clientSource.ShouldContain("long postId");
    }

    [Fact]
    public void Should_Handle_Path_Parameters_In_All_Methods()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        var clientSource = result["PostsClientV1"];
        
        // All methods should accept postId as path parameter
        clientSource.ShouldContain("HttpMethod.Get, $\"posts/{postId}\"");
        clientSource.ShouldContain("HttpMethod.Put, $\"posts/{postId}\"");
        clientSource.ShouldContain("HttpMethod.Patch, $\"posts/{postId}\"");
        clientSource.ShouldContain("HttpMethod.Get, $\"posts/{postId}/comments\"");
    }

    [Fact]
    public void Should_Use_Correct_Version_In_Client_Name()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        // Version is 1.5.0, should use first character (1)
        result.Keys.ShouldContain("PostsClientV1");
        var clientSource = result["PostsClientV1"];
        
        clientSource.ShouldContain("namespace BlogApiApiClientV1;");
        clientSource.ShouldContain("public class PostsClientV1");
    }

    [Fact]
    public void Should_Generate_All_Expected_Models()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Post");
        result.Classes.Keys.ShouldContain("PostUpdate");
        result.Classes.Keys.ShouldContain("PostPatch");
        result.Classes.Keys.ShouldContain("Author");
        result.Classes.Keys.ShouldContain("Comment");
        result.Classes.Keys.ShouldContain("PostStatus");
        
        result.ClassCount.ShouldBe(6);
    }
}
