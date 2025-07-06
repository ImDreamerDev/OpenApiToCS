using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
public record ProfileAnswerReadModel
{
	[JsonPropertyName("answerContent")]
	public string? AnswerContent { get; set; }

	[JsonPropertyName("answerOrder")]
	public int AnswerOrder { get; set; }

	[JsonPropertyName("isCorrect")]
	public bool IsCorrect { get; set; }

	[JsonPropertyName("userAnswered")]
	public bool UserAnswered { get; set; }

}
