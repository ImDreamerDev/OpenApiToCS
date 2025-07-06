using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
public record Tag
{
	[JsonPropertyName("tagName")]
	public string? TagName { get; set; }

	[JsonPropertyName("quizzes")]
	public Quiz[]? Quizzes { get; set; }

}
