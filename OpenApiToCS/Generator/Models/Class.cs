namespace OpenApiToCS.Generator.Models;

public class Class(string name, string ns, string source, List<Property> properties, List<OneOfConverter> oneOfConverters)
{
    public string Name { get; } = name;
    public string Namespace { get; } = ns;
    public string Source { get; } = source;
    public List<Property> Properties { get; } = properties;
    public List<OneOfConverter> OneOfConverters { get; } = oneOfConverters;
}