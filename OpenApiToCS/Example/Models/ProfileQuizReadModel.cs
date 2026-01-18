using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace KLQuizApiClientV1.Models;
public record ProfileQuizReadModel
{
	[JsonPropertyName("quizName")]
	public string? QuizName { get; init; }

	[JsonPropertyName("quizDescription")]
	public string? QuizDescription { get; init; }

	[JsonPropertyName("startTime")]
	public DateTimeOffset StartTime { get; init; }

	[JsonPropertyName("questions")]
	public ProfileQuestionReadModel[]? Questions { get; init; }

	[JsonPropertyName("correctAnswers")]
	public int CorrectAnswers { get; init; }

	[JsonPropertyName("totalQuestions")]
	public int TotalQuestions { get; init; }

	[JsonPropertyName("accuracy")]
	public float Accuracy { get; init; }

}
