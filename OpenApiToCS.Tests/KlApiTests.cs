using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;

namespace OpenApiToCS.Tests;

public class KlApiTests
{
    private readonly OpenApiDocument _document;

    public KlApiTests()
    {
        string json = File.ReadAllText("TestData/kl.json");
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        _document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;
    }

    [Fact]
    public void Should_Deserialize_KL_OpenApi_Document()
    {
        _document.ShouldNotBeNull();
        _document.OpenApiVersion.ShouldBe("3.0.1");
        _document.Info.Title.ShouldBe("KL-Quiz");
        _document.Info.Version.ShouldBe("1.0");
    }

    [Fact]
    public void Should_Generate_Game_Model_With_Complex_Properties()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Game");
        var gameClass = result.Classes["Game"];
        
        gameClass.Source.ShouldContain("public record Game");
        gameClass.Source.ShouldContain("public string? GameCode");
        gameClass.Source.ShouldContain("public Quiz Quiz");
        gameClass.Source.ShouldContain("public User Creator");
        gameClass.Source.ShouldContain("public Player[]? Players");
        gameClass.Source.ShouldContain("public DateTimeOffset? StartTime");
        gameClass.Source.ShouldContain("public DateTimeOffset? EndTime");
        gameClass.Source.ShouldContain("public int QuestionIndex");
        gameClass.Source.ShouldContain("public GameState State");
    }

    [Fact]
    public void Should_Generate_Quiz_Model()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Quiz");
        var quizClass = result.Classes["Quiz"];
        
        quizClass.Source.ShouldContain("public record Quiz");
        quizClass.Source.ShouldContain("public Guid Id");
        quizClass.Source.ShouldContain("public string? QuizName");
        quizClass.Source.ShouldContain("public string? QuizDescription");
        quizClass.Source.ShouldContain("public Question[]? Questions");
    }

    [Fact]
    public void Should_Generate_User_Model()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("User");
        var userClass = result.Classes["User"];
        
        userClass.Source.ShouldContain("public record User");
        userClass.Source.ShouldContain("public long DiscordId");
        userClass.Source.ShouldContain("public string? DisplayName");
        userClass.Source.ShouldContain("public bool IsAdmin");
    }

    [Fact]
    public void Should_Generate_Question_Model_With_Answers()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Question");
        var questionClass = result.Classes["Question"];
        
        questionClass.Source.ShouldContain("public record Question");
        questionClass.Source.ShouldContain("public Guid Id");
        questionClass.Source.ShouldContain("public string? QuestionText");
        questionClass.Source.ShouldContain("public Answer[]? Answers");
    }

    [Fact]
    public void Should_Generate_Answer_Model()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Answer");
        var answerClass = result.Classes["Answer"];
        
        answerClass.Source.ShouldContain("public record Answer");
        answerClass.Source.ShouldContain("public Guid Id");
        answerClass.Source.ShouldContain("public string? AnswerContent");
        answerClass.Source.ShouldContain("public bool IsCorrect");
    }

    [Fact]
    public void Should_Generate_GameState_Enum()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("GameState");
        var gameStateEnum = result.Classes["GameState"];
        
        gameStateEnum.Source.ShouldContain("public enum GameState");
    }

    [Fact]
    public void Should_Generate_Player_Model()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Player");
        var playerClass = result.Classes["Player"];
        
        playerClass.Source.ShouldContain("public record Player");
        playerClass.Source.ShouldContain("public Guid Id");
        playerClass.Source.ShouldContain("public string? Name");
        playerClass.Source.ShouldContain("public bool IsAdmin");
    }

    [Fact]
    public void Should_Generate_GameClient()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("GameClientV1");
        var clientSource = result["GameClientV1"];
        
        clientSource.ShouldContain("public class GameClientV1");
        clientSource.ShouldContain("HttpClient httpClient");
    }

    [Fact]
    public void Should_Generate_QuizClient()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("QuizClientV1");
        var clientSource = result["QuizClientV1"];
        
        clientSource.ShouldContain("public class QuizClientV1");
    }

    [Fact]
    public void Should_Generate_UserClient()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("UserClientV1");
        var clientSource = result["UserClientV1"];
        
        clientSource.ShouldContain("public class UserClientV1");
    }

    [Fact]
    public void Should_Generate_ReadModel_Classes()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("QuizReadModel");
        result.Classes.Keys.ShouldContain("UserReadModel");
        result.Classes.Keys.ShouldContain("UserProfileReadModel");
        result.Classes.Keys.ShouldContain("FileReadModel");
    }

    [Fact]
    public void Should_Use_Correct_Namespace()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        foreach (var apiClass in result.Values)
        {
            apiClass.ShouldContain("namespace KLQuizApiClientV1;");
            apiClass.ShouldContain("using KLQuizApiClientV1.Models;");
        }
    }

    [Fact]
    public void Should_Generate_Expected_Number_Of_Models()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        // Should have at least the main models
        result.ClassCount.ShouldBeGreaterThan(10);
        
        var expectedModels = new[]
        {
            "Game", "Quiz", "User", "Question", "Answer", 
            "Player", "GameState", "QuizReadModel", "UserReadModel"
        };

        foreach (var model in expectedModels)
        {
            result.Classes.Keys.ShouldContain(model);
        }
    }

    [Fact]
    public void Should_Generate_Tag_Model()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Tag");
        var tagClass = result.Classes["Tag"];
        
        tagClass.Source.ShouldContain("public record Tag");
    }

    [Fact]
    public void Should_Handle_Nullable_Properties_Correctly()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        var gameClass = result.Classes["Game"];
        
        // Nullable properties should have ?
        gameClass.Source.ShouldContain("public string? GameCode");
        gameClass.Source.ShouldContain("public Player[]? Players");
        gameClass.Source.ShouldContain("public DateTimeOffset? StartTime");
        gameClass.Source.ShouldContain("public DateTimeOffset? EndTime");
    }
}
