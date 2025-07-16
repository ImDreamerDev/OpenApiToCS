using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace KLQuizApiClientV1.Models;
public record Question
{
	[JsonPropertyName("id")]
	public Guid Id { get; init; }

	[JsonPropertyName("questionText")]
	public string? QuestionText { get; init; }

	[JsonPropertyName("picture")]
	public string? Picture { get; init; }

	[JsonPropertyName("answers")]
	public Answer[]? Answers { get; init; }

	[JsonPropertyName("questionOrder")]
	public int QuestionOrder { get; init; }

	[JsonPropertyName("questionTime")]
	public int QuestionTime { get; init; }

	[JsonPropertyName("score")]
	public int Score { get; init; }

}
