using System.IO;
using System.Linq;
using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;
using Xunit;

namespace OpenApiToCS.Tests;

public class SecurityTests
{
    [Fact]
    public void BearerTokenSecurityScheme_GeneratesOptionsClass()
    {
        // Arrange
        var specJson = @"{
  ""openapi"": ""3.1.0"",
  ""info"": {
    ""title"": ""Secure API"",
    ""version"": ""1.0.0""
  },
  ""servers"": [
    {
      ""url"": ""https://api.example.com""
    }
  ],
  ""paths"": {
    ""/users"": {
      ""get"": {
        ""operationId"": ""getUsers"",
        ""summary"": ""Get all users"",
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""users"": {
                      ""type"": ""array"",
                      ""items"": {
                        ""type"": ""object"",
                        ""properties"": {
                          ""id"": { ""type"": ""string"" },
                          ""name"": { ""type"": ""string"" }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        },
        ""security"": [
          {
            ""bearerAuth"": []
          }
        ]
      }
    }
  },
  ""components"": {
    ""schemas"": {},
    ""securitySchemes"": {
      ""bearerAuth"": {
        ""type"": ""http"",
        ""scheme"": ""bearer"",
        ""description"": ""JWT Bearer token authentication""
      }
    }
  }
}";
        
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        
        var document = JsonSerializer.Deserialize<OpenApiDocument>(specJson, options);
        var dataClasses = new DataClassGenerator(document!).GenerateDataClasses();
        var generator = new OperationGenerator(document!, dataClasses, false);
        
        // Act
        var result = generator.GenerateApiClasses();
        
        // Assert - Should generate API client and options class
        result.Count.ShouldBe(2);
        
        // Find the client and options classes dynamically
        var clientKey = result.Keys.FirstOrDefault(k => !k.Contains("Options"));
        var optionsKey = result.Keys.FirstOrDefault(k => k.Contains("Options"));
        
        clientKey.ShouldNotBeNull();
        optionsKey.ShouldNotBeNull();
        
        var clientCode = result[clientKey];
        var optionsCode = result[optionsKey];
        
        // Verify client has options parameter
        clientCode.ShouldContain(optionsKey);
        clientCode.ShouldContain($"public {clientKey}(HttpClient httpClient, {optionsKey}? options = null)");
        clientCode.ShouldContain($"_options = options ?? new {optionsKey}();");
        
        // Verify options class has bearer auth property
        optionsCode.ShouldContain("public string? BearerAuth { get; set; }");
        optionsCode.ShouldContain("JWT Bearer token authentication");
        
        // Verify security is applied in operation
        clientCode.ShouldContain("Authorization");
        clientCode.ShouldContain("Bearer");
        clientCode.ShouldContain("_options.BearerAuth");
    }
    
    [Fact]
    public void ApiKeySecurityScheme_GeneratesOptionsClass()
    {
        // Arrange
        var specJson = @"{
  ""openapi"": ""3.1.0"",
  ""info"": {
    ""title"": ""API Key Protected API"",
    ""version"": ""1.0.0""
  },
  ""servers"": [
    {
      ""url"": ""https://api.example.com""
    }
  ],
  ""paths"": {
    ""/data"": {
      ""get"": {
        ""operationId"": ""getData"",
        ""summary"": ""Get data"",
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""value"": { ""type"": ""string"" }
                  }
                }
              }
            }
          }
        },
        ""security"": [
          {
            ""apiKey"": []
          }
        ]
      }
    }
  },
  ""components"": {
    ""schemas"": {},
    ""securitySchemes"": {
      ""apiKey"": {
        ""type"": ""apiKey"",
        ""in"": ""header"",
        ""name"": ""X-API-Key"",
        ""description"": ""API Key for authentication""
      }
    }
  }
}";
        
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        
        var document = JsonSerializer.Deserialize<OpenApiDocument>(specJson, options);
        var dataClasses = new DataClassGenerator(document!).GenerateDataClasses();
        var generator = new OperationGenerator(document!, dataClasses, false);
        
        // Act
        var result = generator.GenerateApiClasses();
        
        // Assert
        result.Count.ShouldBe(2);
        
        // Find the client and options classes dynamically
        var clientKey = result.Keys.FirstOrDefault(k => !k.Contains("Options"));
        var optionsKey = result.Keys.FirstOrDefault(k => k.Contains("Options"));
        
        clientKey.ShouldNotBeNull();
        optionsKey.ShouldNotBeNull();
        
        var clientCode = result[clientKey];
        var optionsCode = result[optionsKey];
        
        // Verify options class has API key property
        optionsCode.ShouldContain("public string? ApiKey { get; set; }");
        optionsCode.ShouldContain("API Key for authentication");
        
        // Verify security is applied with custom header
        clientCode.ShouldContain("X-API-Key");
        clientCode.ShouldContain("_options.ApiKey");
    }
    
    [Fact]
    public void MultipleSecuritySchemes_GeneratesAllProperties()
    {
        // Arrange
        var specJson = @"{
  ""openapi"": ""3.1.0"",
  ""info"": {
    ""title"": ""Multi-Auth API"",
    ""version"": ""1.0.0""
  },
  ""servers"": [
    {
      ""url"": ""https://api.example.com""
    }
  ],
  ""paths"": {
    ""/admin"": {
      ""get"": {
        ""operationId"": ""getAdminData"",
        ""summary"": ""Get admin data"",
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""success"": { ""type"": ""boolean"" }
                  }
                }
              }
            }
          }
        }
      }
    }
  },
  ""components"": {
    ""schemas"": {},
    ""securitySchemes"": {
      ""bearerAuth"": {
        ""type"": ""http"",
        ""scheme"": ""bearer""
      },
      ""apiKey"": {
        ""type"": ""apiKey"",
        ""in"": ""header"",
        ""name"": ""X-API-Key""
      },
      ""basicAuth"": {
        ""type"": ""http"",
        ""scheme"": ""basic""
      }
    }
  }
}";
        
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        
        var document = JsonSerializer.Deserialize<OpenApiDocument>(specJson, options);
        var dataClasses = new DataClassGenerator(document!).GenerateDataClasses();
        var generator = new OperationGenerator(document!, dataClasses, false);
        
        // Act
        var result = generator.GenerateApiClasses();
        
        // Debug: print all keys
        Console.WriteLine("Generated classes: " + string.Join(", ", result.Keys));
        
        // Assert
        var optionsKey = result.Keys.FirstOrDefault(k => k.Contains("Options"));
        optionsKey.ShouldNotBeNull();
        var optionsCode = result[optionsKey];
        
        // Verify all three security schemes are in options
        optionsCode.ShouldContain("public string? BearerAuth { get; set; }");
        optionsCode.ShouldContain("public string? ApiKey { get; set; }");
        optionsCode.ShouldContain("public string? BasicAuth { get; set; }");
        
        var clientKey = result.Keys.FirstOrDefault(k => !k.Contains("Options"));
        clientKey.ShouldNotBeNull();
        var clientCode = result[clientKey];
        
        // Verify all three are applied
        clientCode.ShouldContain("Bearer");
        clientCode.ShouldContain("Basic");
        clientCode.ShouldContain("X-API-Key");
    }
    
    [Fact]
    public void NoSecuritySchemes_DoesNotGenerateOptions()
    {
        // Arrange
        var specJson = @"{
  ""openapi"": ""3.1.0"",
  ""info"": {
    ""title"": ""Public API"",
    ""version"": ""1.0.0""
  },
  ""servers"": [
    {
      ""url"": ""https://api.example.com""
    }
  ],
  ""paths"": {
    ""/public"": {
      ""get"": {
        ""operationId"": ""getPublicData"",
        ""summary"": ""Get public data"",
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""success"": { ""type"": ""boolean"" }
                  }
                }
              }
            }
          }
        }
      }
    }
  },
  ""components"": {
    ""schemas"": {}
  }
}";
        
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        
        var document = JsonSerializer.Deserialize<OpenApiDocument>(specJson, options);
        var dataClasses = new DataClassGenerator(document!).GenerateDataClasses();
        var generator = new OperationGenerator(document!, dataClasses, false);
        
        // Act
        var result = generator.GenerateApiClasses();
        
        // Assert - Only API client, no options
        result.ShouldHaveSingleItem();
        var clientKey = result.Keys.First();
        clientKey.ShouldNotContain("Options");
    }
}
