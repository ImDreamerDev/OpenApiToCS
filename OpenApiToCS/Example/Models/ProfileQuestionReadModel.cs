using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
public record ProfileQuestionReadModel
{
	[JsonPropertyName("questionText")]
	public string? QuestionText { get; set; }

	[JsonPropertyName("picture")]
	public string? Picture { get; set; }

	[JsonPropertyName("questionOrder")]
	public int QuestionOrder { get; set; }

	[JsonPropertyName("score")]
	public int Score { get; set; }

	[JsonPropertyName("answers")]
	public ProfileAnswerReadModel[]? Answers { get; set; }

}
