using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
public record Player
{
	[JsonPropertyName("id")]
	public Guid Id { get; set; }

	[JsonPropertyName("connectionId")]
	public string? ConnectionId { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("isAdmin")]
	public bool IsAdmin { get; set; }

	[JsonPropertyName("isConnected")]
	public bool IsConnected { get; set; }

}
