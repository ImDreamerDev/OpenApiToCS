using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;
using System.Text.Json;

namespace OpenApiToCS.Tests;

public class HeaderTests
{
    [Fact]
    public void Should_Add_Optional_Headers_To_Request()
    {
        var json = """
        {
          "openapi": "3.0.0",
          "info": {
            "title": "Test API",
            "version": "1.0.0"
          },
          "paths": {
            "/pets": {
              "get": {
                "operationId": "listPets",
                "parameters": [
                  {
                    "name": "Authorization",
                    "in": "header",
                    "required": false,
                    "schema": { "type": "string" }
                  }
                ],
                "responses": {
                  "200": {
                    "description": "Success",
                    "content": {
                      "application/json": {
                        "schema": {
                          "type": "array",
                          "items": { "$ref": "#/components/schemas/Pet" }
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
                  "name": { "type": "string" }
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
        var dataClasses = new DataClassGenerator(document!).GenerateDataClasses();
        var generator = new OperationGenerator(document!, dataClasses, false);
        var apiClasses = generator.GenerateApiClasses();
        
        var petClient = apiClasses.First().Value;
        
        // Should contain optional header parameter in method signature
        petClient.ShouldContain("string? authorization = null");
        
        // Should check if header has value before adding
        petClient.ShouldContain("authorization.HasValue");
        petClient.ShouldContain("httpRequest.Headers.Add(\"Authorization\"");
    }

    [Fact]
    public void Should_Sanitize_API_Title_With_Illegal_Characters()
    {
        var json = """
        {
          "openapi": "3.0.0",
          "info": {
            "title": "My API | Production @2024 #1",
            "version": "1.0.0"
          },
          "paths": {},
          "components": {
            "schemas": {
              "Test": {
                "type": "object",
                "properties": {
                  "name": { "type": "string" }
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
        var dataClasses = new DataClassGenerator(document!).GenerateDataClasses();
        
        var testClass = dataClasses.Classes["Test"];
        
        // Namespace should have illegal chars replaced
        // "My API | Production @2024 #1" becomes "MyAPIProduction20241" (non-alphanumeric removed)
        testClass.Namespace.ShouldContain("MyAPIProduction20241");
        
        // Should not contain illegal characters
        testClass.Namespace.ShouldNotContain("|");
        testClass.Namespace.ShouldNotContain("@");
        testClass.Namespace.ShouldNotContain("#");
    }
}
