using System.Text.Json.Serialization;
using OpenApiToCS.OpenApi;

namespace OpenApiToCS.Generator;

public static partial class Extensions
{
    private static readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

    internal static string ToTitleCase(this string input)
    {
        switch (input)
        {
            case null:
                throw new ArgumentNullException(nameof(input));
            case "":
                throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
            default:
            {
                if (_cache.TryGetValue(input, out string? cachedValue))
                {
                    return cachedValue;
                }

                string result = ManualPascalize(input);
                _cache[input] = result;
                return result;
            }
        }
    }

    private static string ManualPascalize(string input)
    {
        Span<char> buffer = stackalloc char[input.Length];
        var j = 0;
        var capitalize = true;
        // We use a for loop here to avoid allocations from LINQ methods and performance overhead from enumerators.
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c is ' ' or '_' or '-')
            {
                capitalize = true;
                continue;
            }
            buffer[j++] = capitalize && char.IsLower(c) ? char.ToUpperInvariant(c) : c;
            capitalize = false;
        }
        return new string(buffer[..j]);
    }

    internal static string FirstCharToLower(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Create(input.Length,
                input,
                (span, src) =>
                {
                    span[0] = char.ToLowerInvariant(src[0]);
                    src.AsSpan(1).CopyTo(span[1..]);
                })
        };

    [JsonSerializable(typeof(OpenApiDocument))]
    [JsonSerializable(typeof(OpenApiComponents))]
    [JsonSerializable(typeof(OpenApiInfo))]
    [JsonSerializable(typeof(OpenApiOperation))]
    [JsonSerializable(typeof(OpenApiParameter))]
    [JsonSerializable(typeof(OpenApiPath))]
    [JsonSerializable(typeof(OpenApiRequestBody))]
    [JsonSerializable(typeof(OpenApiResponse))]
    [JsonSerializable(typeof(OpenApiSchema))]
    [JsonSerializable(typeof(OpenApiSchemaContainer))]
    [JsonSerializable(typeof(OpenApiSecurityScheme))]
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
    public partial class OpenApiSourceGenerationContext : JsonSerializerContext;
}