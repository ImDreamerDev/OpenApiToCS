using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;

namespace OpenApiToCS.Tests;

/// <summary>
/// Tests for polymorphic types: allOf, oneOf, anyOf
/// </summary>
public class PolymorphicTypesTests
{
    #region AllOf Tests

    [Fact]
    public void AllOf_Should_Merge_Properties_From_Multiple_Schemas()
    {
        // Arrange
        var json = @"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""Test API"",
                ""version"": ""1.0.0""
            },
            ""components"": {
                ""schemas"": {
                    ""Person"": {
                        ""allOf"": [
                            {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""firstName"": { ""type"": ""string"" },
                                    ""lastName"": { ""type"": ""string"" }
                                },
                                ""required"": [""firstName"", ""lastName""]
                            },
                            {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""age"": { ""type"": ""integer"" },
                                    ""email"": { ""type"": ""string"" }
                                }
                            }
                        ]
                    }
                }
            },
            ""paths"": {}
        }";

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;

        // Act
        var dataGenerator = new DataClassGenerator(document);
        var dataClasses = dataGenerator.GenerateDataClasses();

        // Assert
        dataClasses.ClassCount.ShouldBe(1);
        dataClasses.Classes.ShouldContainKey("Person");
        
        var personClass = dataClasses.Classes["Person"];
        Console.WriteLine("===== Generated Person Class =====");
        Console.WriteLine(personClass.Source);
        Console.WriteLine("===================================");
        
        personClass.Source.ShouldContain("FirstName");
        personClass.Source.ShouldContain("LastName");
        personClass.Source.ShouldContain("Age");
        personClass.Source.ShouldContain("Email");
        personClass.Source.ShouldContain("[Required]"); // Should have required attributes
    }

    [Fact]
    public void AllOf_Should_Support_Inheritance_With_Reference()
    {
        // Arrange
        var json = @"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""Test API"",
                ""version"": ""1.0.0""
            },
            ""components"": {
                ""schemas"": {
                    ""Animal"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""name"": { ""type"": ""string"" },
                            ""species"": { ""type"": ""string"" }
                        },
                        ""required"": [""name"", ""species""]
                    },
                    ""Dog"": {
                        ""allOf"": [
                            { ""$ref"": ""#/components/schemas/Animal"" },
                            {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""breed"": { ""type"": ""string"" },
                                    ""goodBoy"": { ""type"": ""boolean"" }
                                }
                            }
                        ]
                    }
                }
            },
            ""paths"": {}
        }";

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;

        // Act
        var dataGenerator = new DataClassGenerator(document);
        var dataClasses = dataGenerator.GenerateDataClasses();

        // Assert
        dataClasses.ClassCount.ShouldBe(2);
        dataClasses.Classes.ShouldContainKey("Animal");
        dataClasses.Classes.ShouldContainKey("Dog");
        
        var dogClass = dataClasses.Classes["Dog"];
        dogClass.Source.ShouldContain(": Animal"); // Should inherit from Animal
        dogClass.Source.ShouldContain("Breed");
        dogClass.Source.ShouldContain("GoodBoy");
    }

    [Fact]
    public void AllOf_Should_Merge_Required_Fields()
    {
        // Arrange
        var json = @"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""Test API"",
                ""version"": ""1.0.0""
            },
            ""components"": {
                ""schemas"": {
                    ""User"": {
                        ""allOf"": [
                            {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""id"": { ""type"": ""integer"" }
                                },
                                ""required"": [""id""]
                            },
                            {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""username"": { ""type"": ""string"" }
                                },
                                ""required"": [""username""]
                            }
                        ]
                    }
                }
            },
            ""paths"": {}
        }";

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;

        // Act
        var dataGenerator = new DataClassGenerator(document);
        var dataClasses = dataGenerator.GenerateDataClasses();

        // Assert
        var userClass = dataClasses.Classes["User"];
        userClass.Source.ShouldContain("[Required]");
        userClass.Source.ShouldContain("Id");
        userClass.Source.ShouldContain("Username");
    }

    #endregion

    #region OneOf Tests

    [Fact]
    public void OneOf_Should_Generate_Base_And_Derived_Classes()
    {
        // Arrange
        var json = @"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""Test API"",
                ""version"": ""1.0.0""
            },
            ""components"": {
                ""schemas"": {
                    ""Pet"": {
                        ""oneOf"": [
                            {
                                ""title"": ""Cat"",
                                ""type"": ""object"",
                                ""properties"": {
                                    ""meow"": { ""type"": ""boolean"" }
                                }
                            },
                            {
                                ""title"": ""Dog"",
                                ""type"": ""object"",
                                ""properties"": {
                                    ""bark"": { ""type"": ""boolean"" }
                                }
                            }
                        ]
                    }
                }
            },
            ""paths"": {}
        }";

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;

        // Act
        var dataGenerator = new DataClassGenerator(document);
        var dataClasses = dataGenerator.GenerateDataClasses();

        // Assert
        dataClasses.ClassCount.ShouldBe(3); // Pet (base), Cat, Dog
        dataClasses.Classes.ShouldContainKey("Pet");
        dataClasses.Classes.ShouldContainKey("Cat");
        dataClasses.Classes.ShouldContainKey("Dog");
        
        var catClass = dataClasses.Classes["Cat"];
        catClass.Source.ShouldContain(": Pet");
        catClass.Source.ShouldContain("Meow");
        
        var dogClass = dataClasses.Classes["Dog"];
        dogClass.Source.ShouldContain(": Pet");
        dogClass.Source.ShouldContain("Bark");
    }

    [Fact]
    public void OneOf_Should_Generate_Variants_Without_Title()
    {
        // Arrange
        var json = @"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""Test API"",
                ""version"": ""1.0.0""
            },
            ""components"": {
                ""schemas"": {
                    ""Response"": {
                        ""oneOf"": [
                            {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""success"": { ""type"": ""boolean"" }
                                }
                            },
                            {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""error"": { ""type"": ""string"" }
                                }
                            }
                        ]
                    }
                }
            },
            ""paths"": {}
        }";

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;

        // Act
        var dataGenerator = new DataClassGenerator(document);
        var dataClasses = dataGenerator.GenerateDataClasses();

        // Assert
        dataClasses.ClassCount.ShouldBe(3); // Response (base), ResponseVariant1, ResponseVariant2
        dataClasses.Classes.ShouldContainKey("Response");
        dataClasses.Classes.ShouldContainKey("ResponseVariant1");
        dataClasses.Classes.ShouldContainKey("ResponseVariant2");
    }

    #endregion

    #region AnyOf Tests

    [Fact]
    public void AnyOf_Should_Generate_Base_And_Derived_Classes()
    {
        // Arrange
        var json = @"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""Test API"",
                ""version"": ""1.0.0""
            },
            ""components"": {
                ""schemas"": {
                    ""SearchResult"": {
                        ""anyOf"": [
                            {
                                ""title"": ""User"",
                                ""type"": ""object"",
                                ""properties"": {
                                    ""username"": { ""type"": ""string"" }
                                }
                            },
                            {
                                ""title"": ""Product"",
                                ""type"": ""object"",
                                ""properties"": {
                                    ""productName"": { ""type"": ""string"" }
                                }
                            }
                        ]
                    }
                }
            },
            ""paths"": {}
        }";

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;

        // Act
        var dataGenerator = new DataClassGenerator(document);
        var dataClasses = dataGenerator.GenerateDataClasses();

        // Assert
        dataClasses.ClassCount.ShouldBe(3); // SearchResult (base), User, Product
        dataClasses.Classes.ShouldContainKey("SearchResult");
        dataClasses.Classes.ShouldContainKey("User");
        dataClasses.Classes.ShouldContainKey("Product");
        
        var userClass = dataClasses.Classes["User"];
        userClass.Source.ShouldContain(": SearchResult");
        userClass.Source.ShouldContain("Username");
        
        var productClass = dataClasses.Classes["Product"];
        productClass.Source.ShouldContain(": SearchResult");
        productClass.Source.ShouldContain("ProductName");
    }

    [Fact]
    public void AnyOf_Should_Generate_Variants_Without_Title()
    {
        // Arrange
        var json = @"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""Test API"",
                ""version"": ""1.0.0""
            },
            ""components"": {
                ""schemas"": {
                    ""FlexibleData"": {
                        ""anyOf"": [
                            {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""value"": { ""type"": ""integer"" }
                                }
                            },
                            {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""text"": { ""type"": ""string"" }
                                }
                            }
                        ]
                    }
                }
            },
            ""paths"": {}
        }";

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;

        // Act
        var dataGenerator = new DataClassGenerator(document);
        var dataClasses = dataGenerator.GenerateDataClasses();

        // Assert
        dataClasses.ClassCount.ShouldBe(3); // FlexibleData (base), FlexibleDataVariant1, FlexibleDataVariant2
        dataClasses.Classes.ShouldContainKey("FlexibleData");
        dataClasses.Classes.ShouldContainKey("FlexibleDataVariant1");
        dataClasses.Classes.ShouldContainKey("FlexibleDataVariant2");
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void Should_Handle_Nested_AllOf_In_OneOf()
    {
        // Arrange
        var json = @"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""Test API"",
                ""version"": ""1.0.0""
            },
            ""components"": {
                ""schemas"": {
                    ""Base"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""id"": { ""type"": ""integer"" }
                        }
                    },
                    ""Shape"": {
                        ""oneOf"": [
                            {
                                ""title"": ""Circle"",
                                ""allOf"": [
                                    { ""$ref"": ""#/components/schemas/Base"" },
                                    {
                                        ""type"": ""object"",
                                        ""properties"": {
                                            ""radius"": { ""type"": ""number"" }
                                        }
                                    }
                                ]
                            },
                            {
                                ""title"": ""Square"",
                                ""allOf"": [
                                    { ""$ref"": ""#/components/schemas/Base"" },
                                    {
                                        ""type"": ""object"",
                                        ""properties"": {
                                            ""side"": { ""type"": ""number"" }
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                }
            },
            ""paths"": {}
        }";

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;

        // Act
        var dataGenerator = new DataClassGenerator(document);
        var dataClasses = dataGenerator.GenerateDataClasses();

        // Assert
        dataClasses.ClassCount.ShouldBeGreaterThanOrEqualTo(3); // Base, Shape, Circle, Square
        dataClasses.Classes.ShouldContainKey("Base");
        dataClasses.Classes.ShouldContainKey("Shape");
        dataClasses.Classes.ShouldContainKey("Circle");
        dataClasses.Classes.ShouldContainKey("Square");
    }

    [Fact]
    public void Should_Handle_Multiple_AllOf_References()
    {
        // Arrange
        var json = @"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""Test API"",
                ""version"": ""1.0.0""
            },
            ""components"": {
                ""schemas"": {
                    ""Timestamped"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""createdAt"": { ""type"": ""string"", ""format"": ""date-time"" }
                        }
                    },
                    ""Identifiable"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""id"": { ""type"": ""string"", ""format"": ""uuid"" }
                        }
                    },
                    ""Entity"": {
                        ""allOf"": [
                            { ""$ref"": ""#/components/schemas/Identifiable"" },
                            { ""$ref"": ""#/components/schemas/Timestamped"" },
                            {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""name"": { ""type"": ""string"" }
                                }
                            }
                        ]
                    }
                }
            },
            ""paths"": {}
        }";

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;

        // Act
        var dataGenerator = new DataClassGenerator(document);
        var dataClasses = dataGenerator.GenerateDataClasses();

        // Assert
        dataClasses.ClassCount.ShouldBe(3); // Timestamped, Identifiable, Entity
        
        var entityClass = dataClasses.Classes["Entity"];
        // Should inherit from the first reference and have additional properties
        entityClass.Source.ShouldContain("Name");
    }

    #endregion
}
