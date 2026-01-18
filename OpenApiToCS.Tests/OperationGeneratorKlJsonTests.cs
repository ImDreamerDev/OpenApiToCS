using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;
namespace OpenApiToCS.Tests;

public class OperationGeneratorKlJsonTests
{
    private static OpenApiDocument LoadKlDocument()
    {
        var json = File.ReadAllText("TestData/kl.json");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;
    }

    [Fact]
    public void GenerateApiClasses_Should_Produce_Expected_Clients_And_Methods_For_KlQuiz()
    {
        var doc = LoadKlDocument();
        var dataClasses = new DataClassGenerator(doc).GenerateDataClasses();
        var result = new OperationGenerator(doc, dataClasses).GenerateApiClasses();

        // Check expected client class names
        result.Keys.ShouldContain("GameClientV1");
        result.Keys.ShouldContain("QuizClientV1");
        result.Keys.ShouldContain("UserClientV1");

        // Check representative methods for Game
        var gameClient = result["GameClientV1"];
        gameClient.ShouldContain("public async Task PostGame("); // /Game POST
        gameClient.ShouldContain("public async Task PostStart("); // /Game/{gameCode}/start POST
        gameClient.ShouldContain("public async Task GetGameCode("); // /Game/{gameCode} GET

        // Check representative methods for Quiz
        var quizClient = result["QuizClientV1"];
        quizClient.ShouldContain("public async Task<QuizReadModel[]> GetQuiz("); // /Quiz GET
        quizClient.ShouldContain("public async Task<QuizReadModel> PostQuiz("); // /Quiz POST
        quizClient.ShouldContain("public async Task<Quiz> PatchQuiz("); // /Quiz PATCH
        quizClient.ShouldContain("public async Task DeleteQuiz("); // /Quiz DELETE
        quizClient.ShouldContain("public async Task<QuizReadModel> GetQuizId("); // /Quiz/{quizId} GET

        // Check representative methods for User
        var userClient = result["UserClientV1"];
        userClient.ShouldContain("public async Task GetLogin("); // /User/login GET
        userClient.ShouldContain("public async Task<UserProfileReadModel> GetUserId("); // /User/{userId} GET
        userClient.ShouldContain("public async Task<ProfileQuizReadModel[]> GetQuizzes("); // /User/quizzes GET
        userClient.ShouldContain("public async Task GetGetToken("); // /User/token GET
        userClient.ShouldContain("public async Task GetLogout("); // /User/logout GET
        userClient.ShouldContain("public async Task<UserProfileReadModel> PatchUser("); // /User PATCH
        userClient.ShouldContain("public async Task<UserProfileReadModel> PostUpload("); // /User POST
        userClient.ShouldContain("public async Task<UserProfileReadModel?> PatchColor("); // /User/color PATCH

    }
}