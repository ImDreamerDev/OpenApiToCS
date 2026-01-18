# Contributing to OpenApiToCS

Thank you for your interest in contributing to OpenApiToCS! This document provides guidelines and instructions for contributing.

## Code of Conduct

Be respectful, inclusive, and constructive in all interactions.

## How to Contribute

### Reporting Bugs

Before creating a bug report:
1. Check if the issue already exists in GitHub Issues
2. Test with the latest version
3. Verify it's not a problem with your OpenAPI spec

When creating a bug report, include:
- **Description**: Clear description of the bug
- **Steps to Reproduce**: Minimal steps to reproduce the issue
- **Expected Behavior**: What you expected to happen
- **Actual Behavior**: What actually happened
- **OpenAPI Spec**: Sample spec that reproduces the issue (if possible)
- **Environment**: OS, .NET version, OpenApiToCS version
- **Generated Code**: Relevant portions of the generated code (if applicable)

### Suggesting Features

Feature requests are welcome! Please:
1. Check if the feature has already been requested
2. Describe the use case clearly
3. Explain why it would be useful
4. Provide examples if possible

### Pull Requests

#### Before You Start

- **Discuss large changes**: Open an issue first to discuss major changes
- **One feature per PR**: Keep pull requests focused on a single feature/fix
- **Follow existing patterns**: Match the existing code style and structure

#### Development Setup

1. **Fork and clone the repository**
   ```bash
   git clone https://github.com/yourusername/OpenApiToCS.git
   cd OpenApiToCS
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

#### Making Changes

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Follow C# coding conventions
   - Use meaningful variable and method names
   - Add XML documentation comments for public APIs
   - Keep methods focused and reasonably sized

3. **Add tests**
   - Add tests for new features
   - Update tests if changing existing behavior
   - Ensure all tests pass: `dotnet test`
   - Aim for good test coverage

4. **Update documentation**
   - Update README.md if adding user-facing features
   - Add/update XML comments for code documentation
   - Update CHANGELOG.md with your changes

5. **Commit your changes**
   ```bash
   git add .
   git commit -m "Add feature: description of your changes"
   ```
   
   Good commit messages:
   - Use present tense ("Add feature" not "Added feature")
   - Be descriptive but concise
   - Reference issues when applicable (#123)

6. **Push and create a pull request**
   ```bash
   git push origin feature/your-feature-name
   ```
   
   Then open a PR on GitHub with:
   - Clear title describing the change
   - Description of what changed and why
   - Reference to related issues
   - Screenshots/examples if applicable

## Code Style

### General Guidelines

- Use **4 spaces** for indentation (no tabs)
- Follow **C# naming conventions**:
  - PascalCase for types, methods, properties
  - camelCase for local variables, parameters
  - _camelCase for private fields
- Enable **nullable reference types**
- Use **implicit typing** (`var`) when type is obvious
- Prefer **expression-bodied members** for simple getters/methods
- Use **string interpolation** over concatenation
- Add **XML documentation** for public APIs

### Example

```csharp
/// <summary>
/// Generates C# data classes from OpenAPI schemas.
/// </summary>
/// <param name="document">The OpenAPI document to process.</param>
/// <returns>A collection of generated classes.</returns>
public GeneratedCode GenerateDataClasses(OpenApiDocument document)
{
    var classes = new Dictionary<string, Class>();
    
    foreach (var schema in document.Components.Schemas)
    {
        var generatedClass = GenerateClass(schema);
        classes.Add(schema.Key, generatedClass);
    }
    
    return new GeneratedCode(classes);
}
```

## Testing Guidelines

### Test Organization

Tests are organized by feature:
- `PetStoreApiTests.cs` - Tests for PetStore spec
- `BankingApiTests.cs` - Tests for Banking spec
- `PolymorphicTypesTests.cs` - Tests for allOf/oneOf/anyOf
- `EdgeCaseTests.cs` - Tests for edge cases

### Writing Tests

Use **Shouldly** for assertions:
```csharp
[Fact]
public void Should_Generate_Class_With_Correct_Name()
{
    var result = generator.Generate(spec);
    
    result.Classes.ShouldContainKey("Pet");
    result.Classes["Pet"].Name.ShouldBe("Pet");
    result.Classes["Pet"].Properties.Count.ShouldBe(3);
}
```

### Test Naming

- Use descriptive test names: `Should_Generate_Enum_For_Status_Field`
- Follow pattern: `Should_<ExpectedBehavior>_When_<Condition>` (optional)
- Group related tests in the same class

## Project Structure

```
OpenApiToCS/
├── OpenApiToCS/              # Main project
│   ├── Generator/            # Code generation logic
│   │   ├── DataClassGenerator.cs
│   │   ├── OperationGenerator.cs
│   │   └── Models/           # Generator models
│   ├── OpenApi/              # OpenAPI models
│   ├── Templates/            # Code templates
│   └── Program.cs            # CLI entry point
├── OpenApiToCS.Tests/        # Test project
│   └── TestData/             # Test OpenAPI specs
└── .github/workflows/        # CI/CD workflows
```

## Release Process

(For maintainers)

1. Update version in `OpenApiToCS.csproj`
2. Update `CHANGELOG.md` with release notes
3. Create a git tag: `git tag v1.x.x`
4. Push tag: `git push origin v1.x.x`
5. GitHub Actions will automatically build and publish to NuGet

## Questions?

If you have questions:
- Open a GitHub Issue with the "question" label
- Check existing issues and documentation first

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

Thank you for contributing! 🎉
