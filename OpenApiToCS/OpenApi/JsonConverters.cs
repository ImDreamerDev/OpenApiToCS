using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenApiToCS.OpenApi;

/// <summary>
/// Represents a value that can be either a string or an array of strings.
/// Used for OpenAPI 3.1 type field which can be "string" or ["string", "null"].
/// </summary>
public class StringOrArray
{
    public bool IsArray { get; }
    public string? Value { get; }
    public List<string> Values { get; }

    private StringOrArray(string value)
    {
        IsArray = false;
        Value = value;
        Values = new List<string> { value };
    }

    private StringOrArray(List<string> values)
    {
        IsArray = true;
        Values = values;
        Value = values.FirstOrDefault();
    }

    public static StringOrArray FromString(string value) => new(value);
    public static StringOrArray FromArray(List<string> values) => new(values);
    
    public static implicit operator StringOrArray(string value) => FromString(value);
    
    public static bool operator ==(StringOrArray? left, string? right)
    {
        if (left is null) return right is null;
        if (right is null) return false;
        return !left.IsArray && left.Value == right;
    }
    
    public static bool operator !=(StringOrArray? left, string? right) => !(left == right);
    
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            string str => !IsArray && Value == str,
            StringOrArray other => IsArray == other.IsArray && (IsArray ? Values.SequenceEqual(other.Values) : Value == other.Value),
            _ => false
        };
    }
    
    public override int GetHashCode() => IsArray ? Values.GetHashCode() : Value?.GetHashCode() ?? 0;
    
    public override string ToString() => IsArray ? $"[{string.Join(", ", Values)}]" : Value ?? "";
}

/// <summary>
/// JSON converter for StringOrArray to handle both string and array deserialization.
/// </summary>
public class StringOrArrayConverter : JsonConverter<StringOrArray>
{
    public override StringOrArray? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return StringOrArray.FromString(reader.GetString()!);
        }
        
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var values = new List<string>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;
                    
                if (reader.TokenType == JsonTokenType.String)
                {
                    values.Add(reader.GetString()!);
                }
            }
            return StringOrArray.FromArray(values);
        }
        
        return null;
    }

    public override void Write(Utf8JsonWriter writer, StringOrArray value, JsonSerializerOptions options)
    {
        if (value.IsArray)
        {
            writer.WriteStartArray();
            foreach (var item in value.Values)
            {
                writer.WriteStringValue(item);
            }
            writer.WriteEndArray();
        }
        else
        {
            writer.WriteStringValue(value.Value);
        }
    }
}

/// <summary>
/// Represents exclusiveMinimum/exclusiveMaximum which can be boolean (3.0) or numeric (3.1).
/// </summary>
public class ExclusiveValue
{
    public bool IsBoolean { get; }
    public bool BoolValue { get; }
    public decimal? NumericValue { get; }

    private ExclusiveValue(bool value)
    {
        IsBoolean = true;
        BoolValue = value;
    }

    private ExclusiveValue(decimal value)
    {
        IsBoolean = false;
        NumericValue = value;
    }

    public static ExclusiveValue FromBoolean(bool value) => new(value);
    public static ExclusiveValue FromNumeric(decimal value) => new(value);
    
    public override string ToString() => IsBoolean ? BoolValue.ToString() : NumericValue?.ToString() ?? "";
}

/// <summary>
/// JSON converter for ExclusiveValue to handle both boolean and numeric formats.
/// </summary>
public class ExclusiveMinMaxConverter : JsonConverter<ExclusiveValue>
{
    public override ExclusiveValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.True)
        {
            return ExclusiveValue.FromBoolean(true);
        }
        
        if (reader.TokenType == JsonTokenType.False)
        {
            return ExclusiveValue.FromBoolean(false);
        }
        
        if (reader.TokenType == JsonTokenType.Number)
        {
            return ExclusiveValue.FromNumeric(reader.GetDecimal());
        }
        
        return null;
    }

    public override void Write(Utf8JsonWriter writer, ExclusiveValue value, JsonSerializerOptions options)
    {
        if (value.IsBoolean)
        {
            writer.WriteBooleanValue(value.BoolValue);
        }
        else if (value.NumericValue.HasValue)
        {
            writer.WriteNumberValue(value.NumericValue.Value);
        }
    }
}
