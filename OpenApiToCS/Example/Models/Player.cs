using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace KLQuizApiClientV1.Models;
public record Player
{
	[JsonPropertyName("id")]
	public Guid Id { get; init; }

	[JsonPropertyName("connectionId")]
	public string? ConnectionId { get; init; }

	[JsonPropertyName("name")]
	public string? Name { get; init; }

	[JsonPropertyName("isAdmin")]
	public bool IsAdmin { get; init; }

	[JsonPropertyName("isConnected")]
	public bool IsConnected { get; init; }

}
