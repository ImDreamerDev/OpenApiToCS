using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
public record UserProfileReadModel
{
	[JsonPropertyName("displayName")]
	public string? DisplayName { get; set; }

	[JsonPropertyName("avatar")]
	public string? Avatar { get; set; }

	[JsonPropertyName("crystals")]
	public string? Crystals { get; set; }

	[JsonPropertyName("quizCount")]
	public int QuizCount { get; set; }

	[JsonPropertyName("correctAnswers")]
	public int CorrectAnswers { get; set; }

	[JsonPropertyName("averageCorrectAnswers")]
	public float AverageCorrectAnswers { get; set; }

	[JsonPropertyName("creationTime")]
	public DateTimeOffset CreationTime { get; set; }

}
