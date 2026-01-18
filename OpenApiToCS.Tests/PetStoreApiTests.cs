using System.Net;
using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;

namespace OpenApiToCS.Tests;

public class PetStoreApiTests
{
    private readonly OpenApiDocument _document;

    public PetStoreApiTests()
    {
        string json = File.ReadAllText("TestData/petstore.json");
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        _document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;
    }

    [Fact]
    public void Should_Deserialize_PetStore_OpenApi_Document()
    {
        _document.ShouldNotBeNull();
        _document.OpenApiVersion.ShouldBe("3.0.0");
        _document.Info.Title.ShouldBe("Pet Store API");
        _document.Info.Version.ShouldBe("1.0.0");
    }

    [Fact]
    public void Should_Generate_Pet_Model_With_Required_Properties()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Pet");
        var petClass = result.Classes["Pet"];
        
        petClass.Source.ShouldContain("public record Pet");
        petClass.Source.ShouldContain("[Required]");
        petClass.Source.ShouldContain("public Guid Id");
        petClass.Source.ShouldContain("public string Name");
        petClass.Source.ShouldContain("public string? Tag");
        petClass.Source.ShouldContain("public PetStatus Status");
        
        petClass.Properties.Count.ShouldBe(4);
        petClass.Properties.First(p => p.Name == "Id").IsRequired.ShouldBeTrue();
        petClass.Properties.First(p => p.Name == "Name").IsRequired.ShouldBeTrue();
        petClass.Properties.First(p => p.Name == "Tag").IsNullable.ShouldBeTrue();
    }

    [Fact]
    public void Should_Generate_PetStatus_Enum()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("PetStatus");
        var enumClass = result.Classes["PetStatus"];
        
        enumClass.Source.ShouldContain("public enum PetStatus");
        enumClass.Source.ShouldContain("available,");
        enumClass.Source.ShouldContain("pending,");
        enumClass.Source.ShouldContain("sold,");
    }

    [Fact]
    public void Should_Generate_NewPet_Model()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("NewPet");
        var newPetClass = result.Classes["NewPet"];
        
        newPetClass.Source.ShouldContain("public record NewPet");
        newPetClass.Source.ShouldContain("public string Name");
        newPetClass.Source.ShouldContain("public string Tag");
        newPetClass.Properties.First(p => p.Name == "Name").IsRequired.ShouldBeTrue();
    }

    [Fact]
    public void Should_Generate_PetsClient_With_List_Method()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("PetsClientV1");
        var clientSource = result["PetsClientV1"];
        
        clientSource.ShouldContain("public class PetsClientV1");
        clientSource.ShouldContain("public async Task<Pet[]> GetPets(");
        clientSource.ShouldContain("int? limit = null");
    }

    [Fact]
    public void Should_Generate_PetsClient_With_Create_Method()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        var clientSource = result["PetsClientV1"];
        
        clientSource.ShouldContain("public async Task PostPets(");
        clientSource.ShouldContain("NewPet newPet");
    }

    [Fact]
    public void Should_Generate_PetsClient_With_GetById_Method()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        var clientSource = result["PetsClientV1"];
        
        clientSource.ShouldContain("public async Task<Pet> GetPetId(");
        clientSource.ShouldContain("Guid petId");
    }

    [Fact]
    public void Should_Generate_PetsClient_With_Delete_Method()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        var clientSource = result["PetsClientV1"];
        
        clientSource.ShouldContain("public async Task DeletePetId(");
        clientSource.ShouldContain("Guid petId");
    }

    [Fact]
    public void Should_Use_Correct_Namespace()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        var clientSource = result["PetsClientV1"];
        
        clientSource.ShouldContain("namespace PetStoreApiApiClientV1;");
        clientSource.ShouldContain("using PetStoreApiApiClientV1.Models;");
    }
}
