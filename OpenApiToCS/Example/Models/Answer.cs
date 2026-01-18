using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace KLQuizApiClientV1.Models;
public record Answer
{
	[JsonPropertyName("id")]
	public Guid Id { get; init; }

	[JsonPropertyName("answerContent")]
	public string? AnswerContent { get; init; }

	[JsonPropertyName("answerOrder")]
	public int AnswerOrder { get; init; }

	[JsonPropertyName("isCorrect")]
	public bool IsCorrect { get; init; }

}
