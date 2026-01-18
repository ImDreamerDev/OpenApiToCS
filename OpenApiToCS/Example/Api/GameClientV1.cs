using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hellang.Middleware.ProblemDetails;
using KLQuizApiClientV1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;

namespace KLQuizApiClientV1;

// Generated API class for Game
public class GameClientV1(HttpClient httpClient)
{
	public async Task PostGame(Quiz quiz, Action<HttpRequestMessage>? configureRequest = null, JsonSerializerOptions? jsonSerializerOptions = null)
	{
		if (jsonSerializerOptions is null)
		{
			jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
			jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
		}
		var queryBuilder = new QueryBuilder();
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "/Game" + queryBuilder);
		httpRequest.Content = JsonContent.Create(quiz, options: jsonSerializerOptions);
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			return;
		}
		await HandleError(response, $"/Game");
		throw new UnreachableException("This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.");
	}

	public async Task PostStart(string gameCode, Action<HttpRequestMessage>? configureRequest = null)
	{
		var queryBuilder = new QueryBuilder();
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "/Game/{gameCode}/start" + queryBuilder);
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			return;
		}
		await HandleError(response, $"/Game/{gameCode}/start");
		throw new UnreachableException("This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.");
	}

	public async Task GetGameCode(string gameCode, Action<HttpRequestMessage>? configureRequest = null)
	{
		var queryBuilder = new QueryBuilder();
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, "/Game/{gameCode}" + queryBuilder);
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			return;
		}
		await HandleError(response, $"/Game/{gameCode}");
		throw new UnreachableException("This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.");
	}

	private static async Task HandleError(HttpResponseMessage response, string path)
	{
		string errorContent = await response.Content.ReadAsStringAsync();
		ProblemDetails? problemDetails = JsonSerializer.Deserialize<ProblemDetails>(errorContent);
		if (problemDetails is not null)
		{
			throw new ProblemDetailsException(problemDetails);
		}
		if (string.IsNullOrEmpty(errorContent) is false)
		{
			problemDetails = new ProblemDetails()
			{
				Status = (int?)response.StatusCode,
				Title = $"Call to {path} failed with status code {response.StatusCode}",
				Detail = errorContent
			};
			throw new ProblemDetailsException(problemDetails);
		}
		response.EnsureSuccessStatusCode();
	}
}
