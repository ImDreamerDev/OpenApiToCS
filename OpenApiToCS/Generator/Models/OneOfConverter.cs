namespace OpenApiToCS.Generator.Models;

public class OneOfConverter(string name, string ns, string source, List<Class> oneOfClasses)
{
    public string Name { get; init; } = name;
    public string Namespace { get; init; } = ns;
    public string Source { get; init; } = source;
    public List<Class> OneOfs { get; init; } = oneOfClasses;
}