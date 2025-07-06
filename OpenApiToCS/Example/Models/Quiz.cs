using System.ComponentModel.DataAnnotations;
using KLQuizApiClientV1.Models;
using KLQuizApiClientV1.Models;
using System.Text.Json.Serialization;
public record Quiz
{
	[JsonPropertyName("id")]
	public Guid Id { get; set; }

	[JsonPropertyName("quizName")]
	public string? QuizName { get; set; }

	[JsonPropertyName("quizDescription")]
	public string? QuizDescription { get; set; }

	[JsonPropertyName("questions")]
	public Question[]? Questions { get; set; }

	[JsonPropertyName("tags")]
	public Tag[]? Tags { get; set; }

	[JsonPropertyName("creator")]
	public User Creator { get; set; }

	[JsonPropertyName("creationTime")]
	public DateTimeOffset CreationTime { get; set; }

	[JsonPropertyName("updateTime")]
	public DateTimeOffset UpdateTime { get; set; }

	[JsonPropertyName("defaultTime")]
	public int DefaultTime { get; set; }

	[JsonPropertyName("defaultScore")]
	public int DefaultScore { get; set; }

	[JsonPropertyName("snapshotOf")]
	public Quiz SnapshotOf { get; set; }

}
