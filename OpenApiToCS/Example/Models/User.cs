using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace KLQuizApiClientV1.Models;
public record User
{
	[JsonPropertyName("discordId")]
	public long DiscordId { get; init; }

	[JsonPropertyName("displayName")]
	public string? DisplayName { get; init; }

	[JsonPropertyName("isAdmin")]
	public bool IsAdmin { get; init; }

	[JsonPropertyName("games")]
	public Game[]? Games { get; init; }

	[JsonPropertyName("crystals")]
	public long Crystals { get; init; }

	[JsonPropertyName("creationTime")]
	public DateTimeOffset CreationTime { get; init; }

	[JsonPropertyName("lastLogin")]
	public DateTimeOffset LastLogin { get; init; }

	[JsonPropertyName("avatar")]
	public string? Avatar { get; init; }

	[JsonPropertyName("color")]
	public string? Color { get; init; }

}
