using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace KLQuizApiClientV1.Models;
public record ProfileQuestionReadModel
{
	[JsonPropertyName("questionText")]
	public string? QuestionText { get; init; }

	[JsonPropertyName("picture")]
	public string? Picture { get; init; }

	[JsonPropertyName("questionOrder")]
	public int QuestionOrder { get; init; }

	[JsonPropertyName("score")]
	public int Score { get; init; }

	[JsonPropertyName("answers")]
	public ProfileAnswerReadModel[]? Answers { get; init; }

}
