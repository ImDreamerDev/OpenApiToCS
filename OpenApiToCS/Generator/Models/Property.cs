namespace OpenApiToCS.Generator.Models;

public class Property(string name, string type, bool isRequired, bool isNullable, string? description = null)
{
    public string Name { get; } = name;
    public string Type { get; } = type;
    public bool IsRequired { get; } = isRequired;
    public bool IsNullable { get; } = isNullable;
    public string? Description { get; } = description;
}