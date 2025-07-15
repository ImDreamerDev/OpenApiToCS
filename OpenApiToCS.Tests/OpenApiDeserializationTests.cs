using System.Net;
using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;

namespace OpenApiToCS.Tests;

public class OpenApiDeserializationTests
{

    [Fact]
    public void Should_Deeply_Deserialize_OpenApiDocument()
    {
        string json = File.ReadAllText("TestData/kl.json");
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        OpenApiDocument? doc = JsonSerializer.Deserialize<OpenApiDocument>(json, options);
        doc.ShouldNotBeNull();

        // Top-level
        doc.OpenApiVersion.ShouldBe("3.0.1");
        doc.Info.ShouldNotBeNull();
        doc.Info.Title.ShouldBe("KL-Quiz");
        doc.Info.Version.ShouldBe("1.0");
        doc.Paths.ShouldNotBeNull();
        doc.Components.ShouldNotBeNull();

        // Paths
        doc.Paths.Keys.ShouldContain("/Game");
        OpenApiPath gamePath = doc.Paths["/Game"];
        gamePath.Post.ShouldNotBeNull();
        gamePath.Post.RequestBody.ShouldNotBeNull();
        gamePath.Post.RequestBody.Content.Keys.ShouldContain("application/json");
        gamePath.Post.RequestBody.Content["application/json"].Schema.Reference.ShouldBe("#/components/schemas/Quiz");
        gamePath.Post.Responses.ShouldContainKey(HttpStatusCode.OK);
        gamePath.Post.Responses[HttpStatusCode.OK].Description.ShouldBe("Success");

        // Path with parameters
        doc.Paths.Keys.ShouldContain("/Game/{gameCode}/start");
        OpenApiPath startPath = doc.Paths["/Game/{gameCode}/start"];
        startPath.Post.ShouldNotBeNull();
        startPath.Post.Parameters.ShouldNotBeNull();
        startPath.Post.Parameters.Length.ShouldBe(1);
        OpenApiParameter param = startPath.Post.Parameters[0];
        param.Name.ShouldBe("gameCode");
        param.In.ShouldBe("path");
        param.Required.ShouldBeTrue();
        param.Schema.Type.ShouldBe("string");

        // Components/Schemas
        doc.Components.Schemas.ShouldNotBeNull();
        doc.Components.Schemas.Keys.ShouldContain("Quiz");
        OpenApiSchema quizSchema = doc.Components.Schemas["Quiz"];
        quizSchema.Type.ShouldBe("object");
        quizSchema.Properties.ShouldContainKey("id");
        quizSchema.Properties["id"].Type.ShouldBe("string");
        quizSchema.Properties["id"].Format.ShouldBe("uuid");
        quizSchema.Properties.ShouldContainKey("quizName");
        quizSchema.Properties["quizName"].Type.ShouldBe("string");
        quizSchema.Properties["quizName"].Nullable.ShouldBeTrue();

        // Enum schema
        doc.Components.Schemas.Keys.ShouldContain("GameState");
        OpenApiSchema gameStateSchema = doc.Components.Schemas["GameState"];


        gameStateSchema.Enum[0].ToString().ShouldContain("Lobby");
        gameStateSchema.Enum[1].ToString().ShouldBe("Question");
        gameStateSchema.Enum[2].ToString().ShouldBe("Answered");
        gameStateSchema.Enum[3].ToString().ShouldBe("Leaderboard");
        gameStateSchema.Enum[4].ToString().ShouldBe("Finished");
        gameStateSchema.Type.ShouldBe("integer");
        gameStateSchema.Format.ShouldBe("int32");

        // Another schema
        doc.Components.Schemas.Keys.ShouldContain("User");
        OpenApiSchema userSchema = doc.Components.Schemas["User"];
        userSchema.Type.ShouldBe("object");
        userSchema.Properties.ShouldContainKey("discordId");
        userSchema.Properties["discordId"].Type.ShouldBe("integer");
        userSchema.Properties["discordId"].Format.ShouldBe("int64");
        userSchema.Properties.ShouldContainKey("displayName");
        userSchema.Properties["displayName"].Type.ShouldBe("string");
        userSchema.Properties["displayName"].Nullable.ShouldBeTrue();
    }

    [Fact]
    public void Should_Fail_On_Invalid_Json()
    {
        const string invalidJson = "{ not valid json }";
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        Should.Throw<JsonException>(() =>
        {
            JsonSerializer.Deserialize<OpenApiDocument>(invalidJson, options);
        });
    }

    [Fact]
    public void Should_Fail_On_Missing_Required_Properties()
    {
        var json = @"{
            ""openapi"": ""3.0.0"",
            ""info"": { ""title"": ""Test API"" },
            ""paths"": {}
        }";
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        Should.Throw<JsonException>(() =>
        {
            JsonSerializer.Deserialize<OpenApiDocument>(json, options);
        });
    }

    [Fact]
    public void Should_Deserialize_With_Empty_Components()
    {
        var json = @"{
        ""openapi"": ""3.0.0"",
        ""info"": { ""title"": ""Test API"", ""version"": ""1.0.0"" },
        ""paths"": {},
        ""components"": {}
    }";
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        OpenApiDocument? doc = JsonSerializer.Deserialize<OpenApiDocument>(json, options);
        doc.ShouldNotBeNull();
        doc.Components.ShouldNotBeNull();
        doc.Components.Schemas.ShouldBeNull();
    }

    [Fact]
    public void Should_Deeply_Deserialize_DAR_OpenApiDocument()
    {
        string json = File.ReadAllText("TestData/DAR.json");
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        OpenApiDocument? doc = JsonSerializer.Deserialize<OpenApiDocument>(json, options);
        doc.ShouldNotBeNull();

        // Top-level
        doc.OpenApiVersion.ShouldBe("3.0.1");
        doc.Info.ShouldNotBeNull();
        doc.Info.Title.ShouldBe("Datafordeleren Haendelser API");
        doc.Info.Version.ShouldBe("1.0");
        doc.Paths.ShouldNotBeNull();
        doc.Components.ShouldNotBeNull();

        // Paths
        doc.Paths.Keys.ShouldContain("/custom");
        OpenApiPath customPath = doc.Paths["/custom"];
        customPath.Get.ShouldNotBeNull();
        customPath.Get.Parameters.ShouldNotBeNull();
        customPath.Get.Parameters.Length.ShouldBeGreaterThan(0);
        customPath.Get.Responses.ShouldContainKey(HttpStatusCode.OK);

        // Components/Schemas
        doc.Components.Schemas.ShouldNotBeNull();
        doc.Components.Schemas.Keys.ShouldContain("EventWrapper");
        doc.Components.Schemas.Keys.ShouldContain("MessageType");

        // EventWrapper schema
        OpenApiSchema eventWrapper = doc.Components.Schemas["EventWrapper"];
        eventWrapper.Type.ShouldBe("object");
        eventWrapper.Properties.ShouldContainKey("Id");
        eventWrapper.Properties["Id"].Type.ShouldBe("number");
        eventWrapper.Properties.ShouldContainKey("Format");
        eventWrapper.Properties["Format"].Type.ShouldBe("string");
        eventWrapper.Properties.ShouldContainKey("Message");
        eventWrapper.Properties["Message"].Reference.ShouldBe("#/components/schemas/MessageType");
        eventWrapper.Properties.ShouldContainKey("Timestamp");
        eventWrapper.Properties["Timestamp"].Type.ShouldBe("string");
        eventWrapper.Properties["Timestamp"].Format.ShouldBe("date-time");
        eventWrapper.Required.ShouldContain("Id");
        eventWrapper.Required.ShouldContain("Message");
        eventWrapper.Required.ShouldContain("Format");
        eventWrapper.Required.ShouldContain("Timestamp");

        // MessageType schema
        OpenApiSchema messageType = doc.Components.Schemas["MessageType"];
        messageType.Type.ShouldBe("object");
        messageType.Properties.ShouldContainKey("Grunddatabesked");
        OpenApiSchema grunddatabesked = messageType.Properties["Grunddatabesked"];
        grunddatabesked.Type.ShouldBe("object");
        grunddatabesked.Properties.ShouldContainKey("Hændelsesbesked");
        OpenApiSchema haendelsesbesked = grunddatabesked.Properties["Hændelsesbesked"];
        haendelsesbesked.Type.ShouldBe("object");
        haendelsesbesked.Properties.ShouldContainKey("Beskeddata");
        OpenApiSchema beskeddata = haendelsesbesked.Properties["Beskeddata"];
        beskeddata.Type.ShouldBe("array");
        beskeddata.Items.ShouldNotBeNull();
        beskeddata.Items.OneOf.ShouldNotBeNull();
        beskeddata.Items.OneOf.Length.ShouldBe(2);

        // OneOf in Beskeddata
        OpenApiSchema objektdata = beskeddata.Items.OneOf[0];
        objektdata.Title.ShouldBe("Objektdata");
        OpenApiSchema objektreference = beskeddata.Items.OneOf[1];
        objektreference.Title.ShouldBe("Objektreference");
        objektreference.Type.ShouldBe("object");
        objektreference.Properties.ShouldContainKey("objektreference");
        objektreference.Properties["objektreference"].Type.ShouldBe("string");

        // Deep nested: Filtreringsdata.Objektregistrering
        haendelsesbesked.Properties.ShouldContainKey("Beskedkuvert");
        OpenApiSchema beskedkuvert = haendelsesbesked.Properties["Beskedkuvert"];
        beskedkuvert.Type.ShouldBe("object");
        beskedkuvert.Properties.ShouldContainKey("Filtreringsdata");
        OpenApiSchema filtreringsdata = beskedkuvert.Properties["Filtreringsdata"];
        filtreringsdata.Type.ShouldBe("object");
        filtreringsdata.Properties.ShouldContainKey("Objektregistrering");
        OpenApiSchema objektregistrering = filtreringsdata.Properties["Objektregistrering"];
        objektregistrering.Type.ShouldBe("array");
        objektregistrering.Items.ShouldNotBeNull();
        objektregistrering.Items.Type.ShouldBe("object");
        objektregistrering.Items.Properties.ShouldContainKey("Stedbestemmelse");
        OpenApiSchema stedbestemmelse = objektregistrering.Items.Properties["Stedbestemmelse"];
        stedbestemmelse.Type.ShouldBe("object");
        stedbestemmelse.OneOf.ShouldNotBeNull();
        stedbestemmelse.OneOf.Length.ShouldBe(2);
        stedbestemmelse.Properties.ShouldContainKey("stedbestemmelseGeometri");
        stedbestemmelse.Properties.ShouldContainKey("stedbestemmelseReference");
    }

}