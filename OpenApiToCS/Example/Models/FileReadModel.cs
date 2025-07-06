using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
public record FileReadModel
{
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("data")]
	public string? Data { get; set; }

}
