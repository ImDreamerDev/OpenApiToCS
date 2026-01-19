using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;
using System.Text.Json;

namespace OpenApiToCS.Tests;

public class OpenApi31Tests
{
    [Fact]
    public void Should_Parse_OpenAPI_31_Document()
    {
        var json = """
        {
          "openapi": "3.1.0",
          "info": {
            "title": "Test API",
            "version": "1.0.0"
          },
          "paths": {},
          "components": {
            "schemas": {
              "Pet": {
                "type": "object",
                "properties": {
                  "name": { "type": ["string", "null"] }
                }
              }
            }
          }
        }
        """;

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options);
        
        document.ShouldNotBeNull();
        document.OpenApiVersion.ShouldBe("3.1.0");
        document.IsOpenApi31.ShouldBeTrue();
    }

    [Fact]
    public void Should_Handle_Type_Array_For_Nullable()
    {
        var json = """
        {
          "openapi": "3.1.0",
          "info": {
            "title": "Test API",
            "version": "1.0.0"
          },
          "paths": {},
          "components": {
            "schemas": {
              "Pet": {
                "type": "object",
                "properties": {
                  "name": { "type": ["string", "null"] }
                }
              }
            }
          }
        }
        """;

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options);
        var schema = document!.Components.Schemas["Pet"];
        var nameProperty = schema.Properties!["name"];
        
        nameProperty.Type.ShouldNotBeNull();
        nameProperty.Type!.IsArray.ShouldBeTrue();
        nameProperty.Type.Values.ShouldContain("string");
        nameProperty.Type.Values.ShouldContain("null");
        nameProperty.IsNullable().ShouldBeTrue();
        nameProperty.GetPrimaryType().ShouldBe("string");
    }

    [Fact]
    public void Should_Handle_Const_Keyword()
    {
        var json = """
        {
          "openapi": "3.1.0",
          "info": {
            "title": "Test API",
            "version": "1.0.0"
          },
          "paths": {},
          "components": {
            "schemas": {
              "Status": {
                "type": "string",
                "const": "active"
              }
            }
          }
        }
        """;

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options);
        var schema = document!.Components.Schemas["Status"];
        
        schema.Const.ShouldNotBeNull();
        schema.Const.ToString().ShouldBe("active");
    }

    [Fact]
    public void Should_Handle_Examples_Plural()
    {
        var json = """
        {
          "openapi": "3.1.0",
          "info": {
            "title": "Test API",
            "version": "1.0.0"
          },
          "paths": {},
          "components": {
            "schemas": {
              "Pet": {
                "type": "object",
                "properties": {
                  "name": { "type": "string" }
                },
                "examples": {
                  "dog": {
                    "value": { "name": "Fido" },
                    "summary": "A dog example"
                  },
                  "cat": {
                    "value": { "name": "Whiskers" }
                  }
                }
              }
            }
          }
        }
        """;

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options);
        var schema = document!.Components.Schemas["Pet"];
        
        schema.Examples.ShouldNotBeNull();
        schema.Examples.Count.ShouldBe(2);
        schema.Examples.ContainsKey("dog").ShouldBeTrue();
        schema.Examples["dog"].Summary.ShouldBe("A dog example");
    }

    [Fact]
    public void Should_Handle_Webhooks()
    {
        var json = """
        {
          "openapi": "3.1.0",
          "info": {
            "title": "Test API",
            "version": "1.0.0"
          },
          "paths": {},
          "webhooks": {
            "newPet": {
              "post": {
                "requestBody": {
                  "content": {
                    "application/json": {
                      "schema": {
                        "type": "object"
                      }
                    }
                  }
                },
                "responses": {
                  "200": {
                    "description": "Success"
                  }
                }
              }
            }
          },
          "components": {
            "schemas": {}
          }
        }
        """;

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options);
        
        document!.Webhooks.ShouldNotBeNull();
        document.Webhooks.Count.ShouldBe(1);
        document.Webhooks.ContainsKey("newPet").ShouldBeTrue();
    }

    [Fact]
    public void Should_Generate_Code_From_31_Spec()
    {
        var json = """
        {
          "openapi": "3.1.0",
          "info": {
            "title": "Pet Store",
            "version": "1.0.0"
          },
          "paths": {
            "/pets": {
              "get": {
                "operationId": "listPets",
                "responses": {
                  "200": {
                    "description": "Success",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/Pet"
                        }
                      }
                    }
                  }
                }
              }
            }
          },
          "components": {
            "schemas": {
              "Pet": {
                "type": "object",
                "properties": {
                  "name": { "type": ["string", "null"] },
                  "age": { "type": "integer" }
                },
                "required": ["name"]
              }
            }
          }
        }
        """;

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options);
        var generator = new DataClassGenerator(document!);
        var result = generator.GenerateDataClasses();
        
        result.Classes.ShouldContainKey("Pet");
        var petClass = result.Classes["Pet"];
        petClass.Source.ShouldContain("public record Pet");
        petClass.Source.ShouldContain("string? Name"); // Nullable due to type array
    }
}
