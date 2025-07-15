using OpenApiToCS.Generator;
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
        var result = generator.GenerateDataClasses(doc);

        result["TestObject"].ShouldContain("[Required]");
        result["TestObject"].ShouldContain("public string Id");
        result["TestObject"].ShouldContain("public string Name");
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

        result["TestObject"].ShouldContain("public string? Id");
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

        result["TestObject"].ShouldContain("[Obsolete(\"This property is deprecated.\")]");
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
        result.Keys.ShouldContain("TestObject");
        result.Keys.ShouldContain("ChoiceOneOf");
        result.Keys.ShouldContain("choiceA");
        result.Keys.ShouldContain("choiceB");
        result.Keys.ShouldContain("ChoiceOneOfOneOfConverterJson");
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

        result.Keys.ShouldContain("TestObject");
        result["TestObject"].ShouldContain("public record TestObject");
        result["TestObject"].ShouldContain("public string Id");
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

        result.Keys.ShouldContain("TestEnum");
        result["TestEnum"].ShouldContain("public enum TestEnum");
        result["TestEnum"].ShouldContain("A,");
        result["TestEnum"].ShouldContain("B,");
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

        result.Keys.ShouldNotContain("Unsupported");
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
        result.Keys.Count.ShouldBe(2);
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
                                                        new OpenApiSchema { Title = "Objektdata", Required =
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
        result.Keys.ShouldContain("EventWrapper");
        result["EventWrapper"].ShouldContain("public record EventWrapper");
        result["EventWrapper"].ShouldContain("[Required]");
        result["EventWrapper"].ShouldContain("public float Id");
        result["EventWrapper"].ShouldContain("public string Format");
        result["EventWrapper"].ShouldContain("public MessageType Message");
        result["EventWrapper"].ShouldContain("public DateTimeOffset Timestamp");

        // Nested reference
        result.Keys.ShouldContain("MessageType");
        result["MessageType"].ShouldContain("public record MessageType");
        result["MessageType"].ShouldContain("public Grunddatabesked Grunddatabesked");

        // Deep nested object
        result.Keys.ShouldContain("Grunddatabesked");
        result["Grunddatabesked"].ShouldContain("public Hændelsesbesked Hændelsesbesked");

        // oneOf handling
        result.Keys.ShouldContain("BeskeddataOneOf");
        result.Keys.ShouldContain("BeskeddataObjektdata");
        result.Keys.ShouldContain("BeskeddataObjektreference");
        result["BeskeddataObjektreference"].ShouldContain("public string objektreference");
    }
}