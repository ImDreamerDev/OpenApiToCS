using System.ComponentModel.DataAnnotations;
using KLQuizApiClientV1.Models;
using KLQuizApiClientV1.Models;
using KLQuizApiClientV1.Models;
using System.Text.Json.Serialization;
namespace KLQuizApiClientV1.Models;
public record Game
{
	[JsonPropertyName("gameCode")]
	public string? GameCode { get; init; }

	[JsonPropertyName("quiz")]
	public Quiz Quiz { get; init; }

	[JsonPropertyName("creator")]
	public User Creator { get; init; }

	[JsonPropertyName("players")]
	public Player[]? Players { get; init; }

	[JsonPropertyName("startTime")]
	public DateTimeOffset? StartTime { get; init; }

	[JsonPropertyName("endTime")]
	public DateTimeOffset? EndTime { get; init; }

	[JsonPropertyName("questionIndex")]
	public int QuestionIndex { get; init; }

	[JsonPropertyName("state")]
	public GameState State { get; init; }

}
