# OpenApiToCS

Generate type-safe C# API clients and models from OpenAPI 3.0 specifications.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/OpenApiToCS.svg)](https://www.nuget.org/packages/OpenApiToCS/)

## Features

✅ **Full OpenAPI 3.0 & 3.1 Support** - Generates code from OpenAPI 3.0 and 3.1 specifications  
✅ **Security Schemes** - Auto-generates options classes for Bearer, API Key, and Basic auth  
✅ **Polymorphic Types** - Complete support for `allOf`, `oneOf`, `anyOf` with discriminators  
✅ **Type Safety** - Strongly-typed models with nullable reference types  
✅ **Rich Type Mappings** - Maps OpenAPI types to appropriate C# types (Guid, DateTimeOffset, byte[], etc.)  
✅ **HTTP Client Generation** - Creates ready-to-use API clients with all endpoints  
✅ **Mock Server Generator** - Generate runnable ASP.NET mock servers for testing  
✅ **Spec Analytics** - Quality analysis and scoring of OpenAPI specifications  
✅ **Template Customization** - Override default code templates with your own  
✅ **MSBuild Integration** - Auto-generate code during build with `.openapi.json` files  
✅ **Problem Details** - Built-in support for RFC 7807 Problem Details  
✅ **CLI Tool** - Professional command-line interface with watch mode  
✅ **Configuration Files** - Customize generation with `openapitocsconfig.json`  
✅ **Validation** - Validates OpenAPI specs before generation  
✅ **Webhooks** - Support for OpenAPI 3.1 webhooks with handler interfaces  
✅ **Comprehensive Tests** - 236 tests covering all major features

## Quick Start

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later

### Installation

**As a .NET tool (recommended):**
```bash
dotnet tool install --global OpenApiToCS
```

**From source:**
```bash
git clone https://github.com/ImDreamerDev/OpenApiToCS.git
cd OpenApiToCS
dotnet build
```

### Basic Usage

```bash
# Generate code from an OpenAPI spec
openapitocs petstore.json

# Specify output directory
openapitocs api.json --output ./generated

# Analyze spec quality
openapitocs api.json --analyze

# Generate mock server for testing
openapitocs api.json --generate-mock-server --mock-output ./mock-server

# Watch for changes and auto-regenerate
openapitocs api.json --watch

# Validate spec before generation
openapitocs api.json --validate --verbose

# Generate with mono client for DI
openapitocs api.json --mono

# Use custom templates
openapitocs api.json --template-dir ./my-templates
```

### Using Configuration File

Create `openapitocsconfig.json`:
```json
{
  "outputDirectory": "./generated",
  "namespace": "MyApi.Client",
  "generateMonoClient": true
}
```

Then run:
```bash
openapitocs api.json
```

## CLI Options

```
Usage:
  openapitocs <input-file> [options]

Arguments:
  <input-file>              Path to OpenAPI JSON file

Options:
  -o, --output <dir>        Output directory (default: ./code)
  -n, --namespace <name>    Root namespace for generated code
  -c, --config <file>       Configuration file (default: openapitocsconfig.json)
  -w, --watch               Watch for changes and regenerate
  --validate                Validate OpenAPI spec and show warnings
  --analyze                 Analyze spec quality and show report
  --generate-mock-server    Generate a runnable mock API server
  --mock-output <dir>       Mock server output directory (default: ./MockServer)
  --template-dir <dir>      Use custom code templates from directory
  --mono                    Generate a single client class for all operations
  --verbose                 Show detailed output
  -h, --help                Show this help
  -v, --version             Show version
```

## Generated Code Examples

### Input: OpenAPI Schema

```json
{
  "openapi": "3.0.0",
  "info": {
    "title": "Pet Store API",
    "version": "1.0.0"
  },
  "paths": {
    "/pets/{petId}": {
      "get": {
        "operationId": "getPetById",
        "parameters": [
          {
            "name": "petId",
            "in": "path",
            "required": true,
            "schema": { "type": "string", "format": "uuid" }
          }
        ],
        "responses": {
          "200": {
            "description": "Success",
            "content": {
              "application/json": {
                "schema": { "$ref": "#/components/schemas/Pet" }
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
          "id": { "type": "string", "format": "uuid" },
          "name": { "type": "string" },
          "status": { "$ref": "#/components/schemas/PetStatus" }
        },
        "required": ["id", "name"]
      },
      "PetStatus": {
        "type": "string",
        "enum": ["available", "pending", "sold"]
      }
    }
  }
}
```

### Output: Generated C# Code

**Pet.cs** (Data Model):
```csharp
namespace PetStoreAPIApiClientV1.Models;

public record Pet
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("status")]
    public PetStatus? Status { get; init; }
}
```

**PetStatus.cs** (Enum):
```csharp
namespace PetStoreAPIApiClientV1.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PetStatus
{
    Available,
    Pending,
    Sold
}
```

**PetClient.cs** (HTTP Client):
```csharp
public class PetClient
{
    private readonly HttpClient _httpClient;
    
    public PetClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<Pet?> GetPetByIdAsync(
        Guid petId,
        Action<HttpRequestMessage>? configureRequest = null,
        bool allowNullOrEmptyResponse = false,
        JsonSerializerOptions? jsonSerializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/pets/{petId}");
        configureRequest?.Invoke(request);
        
        var response = await _httpClient.SendAsync(request, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponseAsync(response, cancellationToken);
        }
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Pet>(content, jsonSerializerOptions);
    }
}
```

## Security Schemes

When your OpenAPI spec includes security schemes, OpenApiToCS automatically generates an options class to configure authentication:

### Supported Security Types

- ✅ **Bearer Token** (`http` with `bearer` scheme)
- ✅ **Basic Auth** (`http` with `basic` scheme)
- ✅ **API Key** (`apiKey` in header)

### Example OpenAPI Spec with Security

```json
{
  "openapi": "3.1.0",
  "components": {
    "securitySchemes": {
      "bearerAuth": {
        "type": "http",
        "scheme": "bearer",
        "description": "JWT Bearer token"
      },
      "apiKey": {
        "type": "apiKey",
        "in": "header",
        "name": "X-API-Key"
      }
    }
  }
}
```

### Generated Options Class

```csharp
namespace MyApiClientV1;

/// <summary>
/// Configuration options for MyApiClient
/// </summary>
public class MyApiClientOptions
{
    /// <summary>
    /// http authentication (bearer)
    /// </summary>
    public string? BearerAuth { get; set; }
    
    /// <summary>
    /// apiKey authentication
    /// </summary>
    public string? ApiKey { get; set; }
}
```

### Using the Generated Client

```csharp
var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com") };

// Configure authentication
var options = new MyApiClientOptions
{
    BearerAuth = "your-jwt-token-here",
    ApiKey = "your-api-key-here"
};

// Create client with options
var client = new MyApiClient(httpClient, options);

// Authentication is automatically applied to all requests
var data = await client.GetDataAsync();
```

The generated client automatically adds the appropriate headers to every request:
- `Authorization: Bearer {token}` for Bearer auth
- `Authorization: Basic {credentials}` for Basic auth  
- Custom header (e.g., `X-API-Key: {key}`) for API Key auth

## Webhooks (OpenAPI 3.1)

OpenApiToCS supports OpenAPI 3.1 webhooks, generating handler interfaces and base classes for receiving webhook events from your API.

### Example OpenAPI Spec with Webhooks

```json
{
  "openapi": "3.1.0",
  "webhooks": {
    "paymentCompleted": {
      "post": {
        "operationId": "onPaymentCompleted",
        "summary": "Notifies when a payment is completed successfully",
        "requestBody": {
          "required": true,
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/PaymentCompletedEvent"
              }
            }
          }
        }
      }
    }
  }
}
```

### Generated Webhook Code

**IPaymentAPIWebhookHandler.cs:**
```csharp
/// <summary>
/// Webhook handler interface for receiving events from the API
/// </summary>
public interface IPaymentAPIWebhookHandler
{
    /// <summary>
    /// Notifies when a payment is completed successfully
    /// </summary>
    Task OnPaymentCompletedAsync(PaymentCompletedEvent payload);
}
```

**PaymentAPIWebhookHandler.cs:**
```csharp
/// <summary>
/// Base webhook handler with default implementations
/// Override methods to handle specific webhook events
/// </summary>
public class PaymentAPIWebhookHandler : IPaymentAPIWebhookHandler
{
    /// <summary>
    /// Notifies when a payment is completed successfully
    /// </summary>
    public virtual Task OnPaymentCompletedAsync(PaymentCompletedEvent payload)
    {
        Console.WriteLine($"Received webhook: OnPaymentCompleted with payload: {payload}");
        return Task.CompletedTask;
    }
}
```

### Using Webhook Handlers

```csharp
// Extend the generated base class
public class MyWebhookHandler : PaymentAPIWebhookHandler
{
    public override async Task OnPaymentCompletedAsync(PaymentCompletedEvent payload)
    {
        // Your custom webhook handling logic
        Console.WriteLine($"Processing payment {payload.PaymentId}");
        await ProcessPayment(payload);
    }
}

// Use in your webhook endpoint
[ApiController]
[Route("webhooks")]
public class WebhookController : ControllerBase
{
    private readonly IPaymentAPIWebhookHandler _handler;
    
    public WebhookController(IPaymentAPIWebhookHandler handler)
    {
        _handler = handler;
    }
    
    [HttpPost("payment-completed")]
    public async Task<IActionResult> PaymentCompleted([FromBody] PaymentCompletedEvent payload)
    {
        await _handler.OnPaymentCompletedAsync(payload);
        return Ok();
    }
}
```

### Webhook Features
- ✅ API-specific naming prevents conflicts (`I{ApiTitle}WebhookHandler`)
- ✅ Generates interface and base implementation
- ✅ Virtual methods for easy overriding
- ✅ Strongly-typed payloads from schemas
- ✅ Preserves summaries and documentation
- ✅ Supports POST, PUT, and PATCH webhooks
- ✅ PascalCase method naming convention

## Type Mappings

OpenAPI to C# type conversions:

| OpenAPI Type | OpenAPI Format | C# Type |
|--------------|----------------|---------|
| `string` | - | `string` |
| `string` | `email` | `string` |
| `string` | `uuid` | `Guid` |
| `string` | `uri` | `Uri` |
| `string` | `date-time` | `DateTimeOffset` |
| `string` | `date` | `DateOnly` |
| `string` | `time` | `TimeOnly` |
| `string` | `binary` | `byte[]` |
| `integer` | - | `int` |
| `integer` | `int32` | `int` |
| `integer` | `int64` | `long` |
| `number` | - | `float` |
| `number` | `float` | `float` |
| `number` | `double` | `double` |
| `number` | `decimal` | `decimal` |
| `boolean` | - | `bool` |
| `array` | - | `T[]` |
| `object` | - | Custom class |
| `enum` | - | C# enum |

## Architecture

The tool uses a template-based approach for code generation:

### Templates

Text templates define the structure of generated code:

- `RecordClass.txt` - C# record classes for data models
- `EnumClass.txt` - C# enum types
- `Property.txt` - Class properties with JSON attributes
- `ApiClient.txt` - HTTP client class structure
- `ApiOperation.txt` - Individual HTTP method implementations
- `OneOfConverter.txt` - Polymorphic JSON converters
- `ErrorHandling.txt` - Error handling with ProblemDetails

### Generators

- **TemplateEngine.cs** - Simple placeholder-based template engine
- **DataClassGenerator.cs** - Generates models from OpenAPI schemas
- **OperationGenerator.cs** - Generates API clients from operations
- **MockServerGenerator.cs** - Generates runnable mock API servers
- **SpecAnalyzer.cs** - Analyzes and scores OpenAPI spec quality
- **BaseGenerator.cs** - Shared utilities and helpers

This template-based approach makes customization easy—just edit the template files instead of modifying C# string concatenation.

## Advanced Features

### Mock Server Generation

Generate a runnable ASP.NET mock server from your OpenAPI spec for rapid prototyping and testing:

```bash
# Generate mock server
openapitocs petstore.json --generate-mock-server --mock-output ./mock-server

# Run the mock server
cd mock-server
dotnet run
```

The generated mock server:
- ✅ Implements all endpoints from your OpenAPI spec
- ✅ Returns sample responses based on schemas
- ✅ Includes CORS support for frontend development
- ✅ Logs all requests to console
- ✅ Ready to run with `dotnet run`

**Example generated endpoint:**
```csharp
app.MapGET("/pets", () =>
{
    Console.WriteLine("[GET] /pets");
    return Results.Ok(new[] { new { id = 1, name = "Example Pet", status = "available" } });
});
```

### Spec Analytics

Analyze your OpenAPI specification quality and get actionable insights:

```bash
openapitocs petstore.json --analyze
```

**Output:**
```
=== OpenAPI Specification Analysis ===

Title: Pet Store API
Version: 1.0.0
Paths: 5
Operations: 12
Schemas: 8 models

Complexity: Medium
- Endpoints: 12
- Models: 8
- Polymorphic types: 2

Score: 85/100 (Grade: B)

Warnings:
- 2 operations missing descriptions
- Consider adding security schemes

===================================
```

The analyzer scores your spec based on:
- Completeness (title, description, version)
- Documentation quality
- Schema definitions
- Security configuration
- Overall complexity

### Template Customization

Override default code templates with your own:

```bash
# Use custom templates
openapitocs api.json --template-dir ./my-templates
```

**Create custom template** (`my-templates/RecordClass.txt`):
```csharp
// {{metadata}}
namespace {{namespace}};

/// <summary>
/// Generated data model
/// </summary>
public record {{className}}
{
{{properties}}
}
```

Available templates to customize:
- `RecordClass.txt` - Data model structure
- `EnumClass.txt` - Enum type structure
- `Property.txt` - Property format
- `ApiClient.txt` - Client class structure
- `ApiOperation.txt` - Operation method format

### MSBuild Integration

Auto-generate code during build by adding `.openapi.json` files to your project:

**Setup:**

1. Install as NuGet package:
```bash
dotnet add package OpenApiToCS
```

2. Add your OpenAPI spec as `MyApi.openapi.json`

3. Build your project:
```bash
dotnet build
```

Code is automatically generated before compilation!

**Configure via MSBuild properties:**
```xml
<PropertyGroup>
  <OpenApiToCSEnabled>true</OpenApiToCSEnabled>
  <OpenApiToCSOutput>Generated</OpenApiToCSOutput>
  <OpenApiToCSNamespace>MyApp.ApiClient</OpenApiToCSNamespace>
  <OpenApiToCSGenerateMonoClient>true</OpenApiToCSGenerateMonoClient>
</PropertyGroup>
```

## Customization

## Configuration File

### Modifying Templates (Deprecated - Use --template-dir instead)

Templates use `{{PLACEHOLDER}}` syntax for variable substitution:

**Example: Customize ApiOperation.txt**
```csharp
// Original
public async Task<{{RETURN_TYPE}}> {{METHOD_NAME}}Async(

// Custom: Add XML documentation
/// <summary>{{DESCRIPTION}}</summary>
public async Task<{{RETURN_TYPE}}> {{METHOD_NAME}}Async(
```

After modifying templates, rebuild and regenerate your code.

### Extending Generators

To add new features:

1. Modify the appropriate generator class
2. Update the corresponding template
3. Add tests in `OpenApiToCS.Tests/`

## CI/CD

The project includes GitHub Actions workflow that:
- ✅ Runs tests on Windows, Linux, and macOS
- ✅ Generates code coverage reports
- ✅ Validates code formatting
- ✅ Packages NuGet on releases

## Supported OpenAPI Features

- ✅ **OpenAPI 3.0 and 3.1** - Full support for both specification versions
- ✅ **Schemas**: object, array, enum, primitive types
- ✅ **References**: `$ref` to components/schemas
- ✅ **Polymorphic Types**: `allOf` (composition/inheritance), `oneOf` (discriminated unions), `anyOf` (flexible unions)
- ✅ **Path parameters, query parameters, request bodies**
- ✅ **Multiple HTTP methods per path**
- ✅ **Response schemas** (200, 201, 204, etc.)
- ✅ **Required/optional properties**
- ✅ **Nested objects and arrays**
- ✅ **Discriminator support** for polymorphic types
- ✅ **OpenAPI 3.1 Features**:
  - `type` as array for nullable (e.g., `["string", "null"]`)
  - `const` keyword for constant values
  - `examples` (plural) with named examples
  - `webhooks` for event-driven APIs
  - `prefixItems` for tuple validation
  - `exclusiveMinimum`/`exclusiveMaximum` as numbers
- ⚠️ **Partial**: Complex nested polymorphic combinations
- ❌ **Not supported**: Callbacks, links, `not` keyword

## Known Limitations

- Does not generate server-side code (client-side only)
- Complex nested polymorphic combinations may need manual adjustment
- Mock server returns simple example data (not fully realistic)

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Guidelines

- Add tests for new features
- Follow existing code style
- Update README for significant changes
- Ensure all tests pass before submitting

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Built with .NET 10.0
- Uses System.Text.Json for OpenAPI parsing
- Inspired by OpenAPI Generator and NSwag

## Roadmap

Future enhancements:
- [ ] YAML OpenAPI support
- [ ] Full polymorphism support (oneOf/anyOf/allOf)
- [ ] Nullable reference type improvements
- [ ] Server-side code generation
- [ ] Interactive CLI with prompts
- [ ] Visual Studio extension
