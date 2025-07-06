using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
public record User
{
	[JsonPropertyName("discordId")]
	public long DiscordId { get; set; }

	[JsonPropertyName("displayName")]
	public string? DisplayName { get; set; }

	[JsonPropertyName("isAdmin")]
	public bool IsAdmin { get; set; }

	[JsonPropertyName("games")]
	public Game[]? Games { get; set; }

	[JsonPropertyName("crystals")]
	public long Crystals { get; set; }

	[JsonPropertyName("creationTime")]
	public DateTimeOffset CreationTime { get; set; }

	[JsonPropertyName("lastLogin")]
	public DateTimeOffset LastLogin { get; set; }

	[JsonPropertyName("avatar")]
	public string? Avatar { get; set; }

	[JsonPropertyName("color")]
	public string? Color { get; set; }

}
