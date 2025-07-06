using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
public record ProfileQuizReadModel
{
	[JsonPropertyName("quizName")]
	public string? QuizName { get; set; }

	[JsonPropertyName("quizDescription")]
	public string? QuizDescription { get; set; }

	[JsonPropertyName("startTime")]
	public DateTimeOffset StartTime { get; set; }

	[JsonPropertyName("questions")]
	public ProfileQuestionReadModel[]? Questions { get; set; }

	[JsonPropertyName("correctAnswers")]
	public int CorrectAnswers { get; set; }

	[JsonPropertyName("totalQuestions")]
	public int TotalQuestions { get; set; }

	[JsonPropertyName("accuracy")]
	public float Accuracy { get; set; }

}
