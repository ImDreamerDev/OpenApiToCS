using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace KLQuizApiClientV1.Models;
public record Tag
{
	[JsonPropertyName("tagName")]
	public string? TagName { get; init; }

	[JsonPropertyName("quizzes")]
	public Quiz[]? Quizzes { get; init; }

}
