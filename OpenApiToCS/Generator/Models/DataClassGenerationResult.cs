namespace OpenApiToCS.Generator.Models;

public class DataClassGenerationResult
{
    public Dictionary<string, Class> Classes { get; } = [];
    public int ClassCount => Classes.Count + Classes.Sum(pair => pair.Value.OneOfConverters.Count);
    public List<OneOfConverter> Converters => Classes.SelectMany(pair => pair.Value.OneOfConverters).ToList();
}