using System.Text.Json.Serialization;
namespace KLQuizApiClientV1.Models;
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum GameState
{
        Lobby,
        Question,
        Answered,
        Leaderboard,
        Finished,
}
