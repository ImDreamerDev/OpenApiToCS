using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace KLQuizApiClientV1.Models;
public record UserReadModel
{
	[JsonPropertyName("discordId")]
	public string? DiscordId { get; init; }

	[JsonPropertyName("displayName")]
	public string? DisplayName { get; init; }

	[JsonPropertyName("crystals")]
	public long Crystals { get; init; }

	[JsonPropertyName("creationTime")]
	public DateTimeOffset CreationTime { get; init; }

	[JsonPropertyName("lastLogin")]
	public DateTimeOffset LastLogin { get; init; }

	[JsonPropertyName("avatar")]
	public string? Avatar { get; init; }

}
