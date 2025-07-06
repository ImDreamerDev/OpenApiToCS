using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
public record UserReadModel
{
	[JsonPropertyName("discordId")]
	public string? DiscordId { get; set; }

	[JsonPropertyName("displayName")]
	public string? DisplayName { get; set; }

	[JsonPropertyName("crystals")]
	public long Crystals { get; set; }

	[JsonPropertyName("creationTime")]
	public DateTimeOffset CreationTime { get; set; }

	[JsonPropertyName("lastLogin")]
	public DateTimeOffset LastLogin { get; set; }

	[JsonPropertyName("avatar")]
	public string? Avatar { get; set; }

}
