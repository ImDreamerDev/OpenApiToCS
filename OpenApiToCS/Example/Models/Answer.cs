using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
public record Answer
{
	[JsonPropertyName("id")]
	public Guid Id { get; set; }

	[JsonPropertyName("answerContent")]
	public string? AnswerContent { get; set; }

	[JsonPropertyName("answerOrder")]
	public int AnswerOrder { get; set; }

	[JsonPropertyName("isCorrect")]
	public bool IsCorrect { get; set; }

}
