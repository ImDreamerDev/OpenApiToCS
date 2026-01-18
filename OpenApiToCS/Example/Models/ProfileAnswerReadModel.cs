using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace KLQuizApiClientV1.Models;
public record ProfileAnswerReadModel
{
	[JsonPropertyName("answerContent")]
	public string? AnswerContent { get; init; }

	[JsonPropertyName("answerOrder")]
	public int AnswerOrder { get; init; }

	[JsonPropertyName("isCorrect")]
	public bool IsCorrect { get; init; }

	[JsonPropertyName("userAnswered")]
	public bool UserAnswered { get; init; }

}
