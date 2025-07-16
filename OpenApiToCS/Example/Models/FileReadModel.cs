using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace KLQuizApiClientV1.Models;
public record FileReadModel
{
	[JsonPropertyName("name")]
	public string? Name { get; init; }

	[JsonPropertyName("data")]
	public string? Data { get; init; }

}
