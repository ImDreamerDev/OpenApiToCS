# OpenApiToCS

OpenApiToCS is a C# tool that generates C# data and API classes from OpenAPI (Swagger) specifications.

## Features

- Parses OpenAPI 3.0 JSON documents
- Generates C# data models and API client classes
- Designed for .NET 8.0

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- (Optional) [JetBrains Rider](https://www.jetbrains.com/rider/) or Visual Studio


### Usage
Run the tool with your OpenAPI JSON file
```sh
dotnet run --project OpenApiToCS/OpenApiToCS.csproj -- <path-to-openapi.json>
```
Generated C# files will be placed in the code/Models and code/Api directories.