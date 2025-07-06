using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Hellang.Middleware.ProblemDetails;
using KLQuizApiClientV1.Models;
using Microsoft.AspNetCore.Mvc;

namespace KLQuizApiClientV1;

// Generated API class for Quiz
public class QuizClientV1(HttpClient httpClient)
{
	public async Task<QuizReadModel[]>GetQuiz(Action<HttpRequestMessage>? configureRequest = null, bool allowNullOrEmptyResponse = false, JsonSerializerOptions? jsonSerializerOptions = null)
	{
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/Quiz");
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<QuizReadModel[]>(options: jsonSerializerOptions);
			if (result is null && allowNullOrEmptyResponse)
			{
				return [];
			}
			return result ?? throw new InvalidOperationException("Failed to deserialize response.");
		}
		await HandleError(response, $"/Quiz");
		throw new UnreachableException("This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.");
	}
	public async Task<QuizReadModel>PostQuiz(Quiz quiz, Action<HttpRequestMessage>? configureRequest = null, bool allowNullOrEmptyResponse = false, JsonSerializerOptions? jsonSerializerOptions = null)
	{
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/Quiz");
		httpRequest.Content = JsonContent.Create(quiz, options: jsonSerializerOptions);
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<QuizReadModel>(options: jsonSerializerOptions);
			if (result is null && allowNullOrEmptyResponse)
			{
				return null!;
			}
			return result ?? throw new InvalidOperationException("Failed to deserialize response.");
		}
		await HandleError(response, $"/Quiz");
		throw new UnreachableException("This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.");
	}
	public async Task DeleteQuiz(Quiz quiz, Action<HttpRequestMessage>? configureRequest = null, JsonSerializerOptions? jsonSerializerOptions = null)
	{
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/Quiz");
		httpRequest.Content = JsonContent.Create(quiz, options: jsonSerializerOptions);
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			return;
		}
		await HandleError(response, $"/Quiz");
		throw new UnreachableException("This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.");
	}
	public async Task<Quiz>PatchQuiz(Quiz quiz, Action<HttpRequestMessage>? configureRequest = null, bool allowNullOrEmptyResponse = false, JsonSerializerOptions? jsonSerializerOptions = null)
	{
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/Quiz");
		httpRequest.Content = JsonContent.Create(quiz, options: jsonSerializerOptions);
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<Quiz>(options: jsonSerializerOptions);
			if (result is null && allowNullOrEmptyResponse)
			{
				return null!;
			}
			return result ?? throw new InvalidOperationException("Failed to deserialize response.");
		}
		await HandleError(response, $"/Quiz");
		throw new UnreachableException("This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.");
	}
	public async Task<QuizReadModel>GetQuizId(Guid quizId, Action<HttpRequestMessage>? configureRequest = null, bool allowNullOrEmptyResponse = false, JsonSerializerOptions? jsonSerializerOptions = null)
	{
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/Quiz/{quizId}");
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<QuizReadModel>(options: jsonSerializerOptions);
			if (result is null && allowNullOrEmptyResponse)
			{
				return null!;
			}
			return result ?? throw new InvalidOperationException("Failed to deserialize response.");
		}
		await HandleError(response, $"/Quiz/{quizId}");
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
