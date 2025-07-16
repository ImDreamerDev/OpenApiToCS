using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace KLQuizApiClientV1.Models;
public record UserProfileReadModel
{
	[JsonPropertyName("displayName")]
	public string? DisplayName { get; init; }

	[JsonPropertyName("avatar")]
	public string? Avatar { get; init; }

	[JsonPropertyName("crystals")]
	public string? Crystals { get; init; }

	[JsonPropertyName("quizCount")]
	public int QuizCount { get; init; }

	[JsonPropertyName("correctAnswers")]
	public int CorrectAnswers { get; init; }

	[JsonPropertyName("averageCorrectAnswers")]
	public float AverageCorrectAnswers { get; init; }

	[JsonPropertyName("creationTime")]
	public DateTimeOffset CreationTime { get; init; }

}
