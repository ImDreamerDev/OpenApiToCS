using System.ComponentModel.DataAnnotations;
using KLQuizApiClientV1.Models;
using KLQuizApiClientV1.Models;
using KLQuizApiClientV1.Models;
using System.Text.Json.Serialization;
public record Game
{
	[JsonPropertyName("gameCode")]
	public string? GameCode { get; set; }

	[JsonPropertyName("quiz")]
	public Quiz Quiz { get; set; }

	[JsonPropertyName("creator")]
	public User Creator { get; set; }

	[JsonPropertyName("players")]
	public Player[]? Players { get; set; }

	[JsonPropertyName("startTime")]
	public DateTimeOffset? StartTime { get; set; }

	[JsonPropertyName("endTime")]
	public DateTimeOffset? EndTime { get; set; }

	[JsonPropertyName("questionIndex")]
	public int QuestionIndex { get; set; }

	[JsonPropertyName("state")]
	public GameState State { get; set; }

}
