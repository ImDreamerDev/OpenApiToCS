using OpenApiToCS.Generator;
using OpenApiToCS.Generator.Models;
using OpenApiToCS.OpenApi;
using Shouldly;

namespace OpenApiToCS.Tests;

public class DataClassGeneratorTests
{

    [Fact]
    public void Should_Mark_Required_Properties_With_Attribute()
    {
        OpenApiDocument doc = new OpenApiDocument
        {
            OpenApiVersion = "3.0.1",
            Info = new OpenApiInfo { Title = "Test", Version = "1.0" },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["TestObject"] = new OpenApiSchema
                    {
                        Type = "object",
                        Required =
                        [
                            "id"
                        ],
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["id"] = new OpenApiSchema { Type = "string" },
                            ["name"] = new OpenApiSchema { Type = "string" }
                        }
                    }
                }
            }
        };

        DataClassGenerator generator = new DataClassGenerator();
        DataClassGenerationResult result = generator.GenerateDataClasses(doc);

        result.Classes["TestObject"].Source.ShouldContain("[Required]");
        result.Classes["TestObject"].Source.ShouldContain("public string Id");
        result.Classes["TestObject"].Source.ShouldContain("public string Name");
    }

    [Fact]
    public void Should_Mark_Nullable_Properties()
    {
        OpenApiDocument doc = new OpenApiDocument
        {
            OpenApiVersion = "3.0.1",
            Info = new OpenApiInfo { Title = "Test", Version = "1.0" },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["TestObject"] = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["id"] = new OpenApiSchema { Type = "string", Nullable = true }
                        }
                    }
                }
            }
        };

        DataClassGenerator generator = new DataClassGenerator();
        var result = generator.GenerateDataClasses(doc);

        result.Classes["TestObject"].Source.ShouldContain("public string? Id");
    }

    [Fact]
    public void Should_Mark_Deprecated_Properties()
    {
        OpenApiDocument doc = new OpenApiDocument
        {
            OpenApiVersion = "3.0.1",
            Info = new OpenApiInfo { Title = "Test", Version = "1.0" },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["TestObject"] = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["oldProp"] = new OpenApiSchema { Type = "string", Deprecated = true }
                        }
                    }
                }
            }
        };

        DataClassGenerator generator = new DataClassGenerator();
        var result = generator.GenerateDataClasses(doc);

        result.Classes["TestObject"].Source.ShouldContain("[Obsolete(\"This property is deprecated.\")]");
    }

    [Fact]
    public void Should_Generate_OneOf_Classes()
    {
        OpenApiDocument doc = new OpenApiDocument
        {
            OpenApiVersion = "3.0.1",
            Info = new OpenApiInfo { Title = "Test", Version = "1.0" },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["TestObject"] = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["choice"] = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema
                                {
                                    OneOf =
                                    [
                                        new OpenApiSchema { Title = "A", Type = "string" },
                                        new OpenApiSchema { Title = "B", Type = "integer" }
                                    ]
                                }
                            }
                        }
                    }
                }
            }
        };

        DataClassGenerator generator = new DataClassGenerator();
        var result = generator.GenerateDataClasses(doc);

        // Should generate TestObject, TestObjectOneOf, TestObjectA, TestObjectB, and a converter
        result.Classes.Keys.ShouldContain("TestObject");

        var testObjectClass = result.Classes["TestObject"];
        testObjectClass.Name.ShouldBe("TestObject");
        testObjectClass.Namespace.ShouldBe("TestApiClientV1.Models");
        testObjectClass.Source.ShouldContain("public record TestObject");
        testObjectClass.Properties.Count.ShouldBe(1);
        var property = testObjectClass.Properties.FirstOrDefault();
        property.ShouldNotBeNull();
        property.Name.ShouldBe("Choice");
        property.Type.ShouldBe("ChoiceOneOf[]");
        property.IsRequired.ShouldBeFalse();
        property.IsNullable.ShouldBeFalse();
        property.Description.ShouldBeNull();
        
        
        var oneOfConverter = testObjectClass.OneOfConverters.FirstOrDefault();
        oneOfConverter.ShouldNotBeNull();
        oneOfConverter.Name.ShouldBe("ChoiceOneOfConverterJson");
        oneOfConverter.Namespace.ShouldBe("TestApiClientV1.Models");
        oneOfConverter.Source.ShouldContain("public class ChoiceOneOfConverterJson : JsonConverter<ChoiceOneOf>");
        
        var oneOfs = oneOfConverter.OneOfs;
        oneOfs.ShouldNotBeNull();
        oneOfs.Count.ShouldBe(3);
        oneOfs[0].Name.ShouldBe("ChoiceOneOf");
        oneOfs[1].Name.ShouldBe("choiceA");
        oneOfs[2].Name.ShouldBe("choiceB");
    }

    [Fact]
    public void Should_Generate_DataClass_For_Object_Schema()
    {
        OpenApiDocument doc = new OpenApiDocument
        {
            OpenApiVersion = "3.0.1",
            Info = new OpenApiInfo { Title = "Test", Version = "1.0" },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["TestObject"] = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["id"] = new OpenApiSchema { Type = "string" }
                        }
                    }
                }
            }
        };

        DataClassGenerator generator = new DataClassGenerator();
        var result = generator.GenerateDataClasses(doc);

        result.Classes.Keys.ShouldContain("TestObject");
        result.Classes["TestObject"].Source.ShouldContain("public record TestObject");
        result.Classes["TestObject"].Source.ShouldContain("public string Id");
    }

    [Fact]
    public void Should_Generate_Enum_For_Enum_Schema()
    {
        OpenApiDocument doc = new OpenApiDocument
        {
            OpenApiVersion = "3.0.1",
            Info = new OpenApiInfo { Title = "Test", Version = "1.0" },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["TestEnum"] = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = ["A", "B"]
                    }
                }
            }
        };

        DataClassGenerator generator = new DataClassGenerator();
        var result = generator.GenerateDataClasses(doc);

        result.Classes.Keys.ShouldContain("TestEnum");
        result.Classes["TestEnum"].Source.ShouldContain("public enum TestEnum");
        result.Classes["TestEnum"].Source.ShouldContain("A,");
        result.Classes["TestEnum"].Source.ShouldContain("B,");
    }

    [Fact]
    public void Should_Skip_Unsupported_Schema_Type()
    {
        OpenApiDocument doc = new OpenApiDocument
        {
            OpenApiVersion = "3.0.1",
            Info = new OpenApiInfo { Title = "Test", Version = "1.0" },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["Unsupported"] = new OpenApiSchema
                    {
                        Type = "array"
                    }
                }
            }
        };

        DataClassGenerator generator = new DataClassGenerator();
        var result = generator.GenerateDataClasses(doc);

        result.Classes.Keys.ShouldNotContain("Unsupported");
    }

    [Fact]
    public void Should_Not_Generate_Duplicate_Schemas()
    {
        OpenApiDocument doc = new OpenApiDocument
        {
            OpenApiVersion = "3.0.1",
            Info = new OpenApiInfo { Title = "Test", Version = "1.0" },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["TestObject"] = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["id"] = new OpenApiSchema { Type = "string" }
                        }
                    },
                    ["TestObjectDuplicate"] = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["id"] = new OpenApiSchema { Type = "string" }
                        }
                    }
                }
            }
        };

        DataClassGenerator generator = new DataClassGenerator();
        var result = generator.GenerateDataClasses(doc);

        // Both keys are different, but class names will be the same after ToTitleCase
        result.Classes.Keys.Count.ShouldBe(2);
    }

    [Fact]
    public void Should_Generate_EventWrapper_And_MessageType_Classes()
    {
        OpenApiDocument doc = new OpenApiDocument
        {
            OpenApiVersion = "3.0.1",
            Info = new OpenApiInfo { Title = "Datafordeleren Haendelser API", Version = "1.0" },
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["EventWrapper"] = new OpenApiSchema
                    {
                        Type = "object",
                        Required =
                        [
                            "Id", "Message", "Format", "Timestamp"
                        ],
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["Format"] = new OpenApiSchema { Type = "string" },
                            ["Id"] = new OpenApiSchema { Type = "number" },
                            ["Message"] = new OpenApiSchema { Reference = "#/components/schemas/MessageType" },
                            ["Timestamp"] = new OpenApiSchema { Type = "string", Format = "date-time" }
                        }
                    },
                    ["MessageType"] = new OpenApiSchema
                    {
                        Type = "object",
                        Required =
                        [
                            "Grunddatabesked"
                        ],
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["Grunddatabesked"] = new OpenApiSchema
                            {
                                Type = "object",
                                Required =
                                [
                                    "Hændelsesbesked"
                                ],
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["Hændelsesbesked"] = new OpenApiSchema
                                    {
                                        Type = "object",
                                        Required =
                                        [
                                            "Beskedkuvert", "beskedID", "beskedversion"
                                        ],
                                        Properties = new Dictionary<string, OpenApiSchema>
                                        {
                                            ["Beskeddata"] = new OpenApiSchema
                                            {
                                                Type = "array",
                                                Items = new OpenApiSchema
                                                {
                                                    OneOf =
                                                    [
                                                        new OpenApiSchema
                                                        {
                                                            Title = "Objektdata", Required =
                                                            [
                                                                "Objektdata"
                                                            ]
                                                        },
                                                        new OpenApiSchema
                                                        {
                                                            Title = "Objektreference",
                                                            Type = "object",
                                                            Required =
                                                            [
                                                                "objektreference"
                                                            ],
                                                            Properties = new Dictionary<string, OpenApiSchema>
                                                            {
                                                                ["objektreference"] = new OpenApiSchema { Type = "string" }
                                                            }
                                                        }
                                                    ]
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        DataClassGenerator generator = new DataClassGenerator();
        var result = generator.GenerateDataClasses(doc);

        // Top-level class
        result.Classes.Keys.ShouldContain("EventWrapper");
        result.Classes["EventWrapper"].Source.ShouldContain("public record EventWrapper");
        result.Classes["EventWrapper"].Source.ShouldContain("[Required]");
        result.Classes["EventWrapper"].Source.ShouldContain("public float Id");
        result.Classes["EventWrapper"].Source.ShouldContain("public string Format");
        result.Classes["EventWrapper"].Source.ShouldContain("public MessageType Message");
        result.Classes["EventWrapper"].Source.ShouldContain("public DateTimeOffset Timestamp");

        // Nested reference
        result.Classes.Keys.ShouldContain("MessageType");
        result.Classes["MessageType"].Source.ShouldContain("public record MessageType");
        result.Classes["MessageType"].Source.ShouldContain("public Grunddatabesked Grunddatabesked");

        // Deep nested object
        result.Classes.Keys.ShouldContain("Grunddatabesked");
        result.Classes["Grunddatabesked"].Source.ShouldContain("public Hændelsesbesked Hændelsesbesked");

        // oneOf handling
        var oneOfConverter = result.Classes["Hændelsesbesked"].OneOfConverters.FirstOrDefault();
        oneOfConverter.ShouldNotBeNull();
        oneOfConverter.Name.ShouldBe("BeskeddataOneOfConverterJson");
        var oneOfs = oneOfConverter.OneOfs;
        oneOfs.ShouldNotBeNull();
        oneOfs.Count.ShouldBe(3);
        oneOfs[0].Name.ShouldBe("BeskeddataOneOf");
        oneOfs[1].Name.ShouldBe("BeskeddataObjektdata");
        oneOfs[2].Name.ShouldBe("BeskeddataObjektreference");
    }
}