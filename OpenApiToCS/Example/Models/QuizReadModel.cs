using System.ComponentModel.DataAnnotations;
using KLQuizApiClientV1.Models;
using System.Text.Json.Serialization;
namespace KLQuizApiClientV1.Models;
public record QuizReadModel
{
	[JsonPropertyName("id")]
	public Guid Id { get; init; }

	[JsonPropertyName("quizName")]
	public string? QuizName { get; init; }

	[JsonPropertyName("quizDescription")]
	public string? QuizDescription { get; init; }

	[JsonPropertyName("questions")]
	public Question[]? Questions { get; init; }

	[JsonPropertyName("tags")]
	public Tag[]? Tags { get; init; }

	[JsonPropertyName("creator")]
	public UserReadModel Creator { get; init; }

	[JsonPropertyName("creationTime")]
	public DateTimeOffset CreationTime { get; init; }

	[JsonPropertyName("updateTime")]
	public DateTimeOffset UpdateTime { get; init; }

	[JsonPropertyName("defaultTime")]
	public int DefaultTime { get; init; }

	[JsonPropertyName("defaultScore")]
	public int DefaultScore { get; init; }

}
