using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Hellang.Middleware.ProblemDetails;
using KLQuizApiClientV1.Models;
using Microsoft.AspNetCore.Mvc;
namespace KLQuizApiClientV1;

// Generated API class for User
public class UserClientV1(HttpClient httpClient)
{
	public async Task GetLogin(string? returnUrl = null,Action<HttpRequestMessage>? configureRequest = null)	{
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/User/login?returnUrl={returnUrl ?? null}");
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			return;
		}
		await HandleError(response, $"/User/login");
		throw new UnreachableException("This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.");
	}
	public async Task<UserProfileReadModel>GetUserId(string userId,Action<HttpRequestMessage>? configureRequest = null, bool allowNullOrEmptyResponse = false, JsonSerializerOptions? jsonSerializerOptions = null)	{
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/User/{userId}");
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<UserProfileReadModel>(options: jsonSerializerOptions);
			if (result is null && allowNullOrEmptyResponse)
			{
				return null!;
			}
			return result ?? throw new InvalidOperationException("Failed to deserialize response.");
		}
		await HandleError(response, $"/User/{userId}");
		throw new UnreachableException("This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.");
	}
	public async Task<ProfileQuizReadModel[]>GetQuizzes(string userId,Action<HttpRequestMessage>? configureRequest = null, bool allowNullOrEmptyResponse = false, JsonSerializerOptions? jsonSerializerOptions = null)	{
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/User/{userId}/quizzes");
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<ProfileQuizReadModel[]>(options: jsonSerializerOptions);
			if (result is null && allowNullOrEmptyResponse)
			{
				return [];
			}
			return result ?? throw new InvalidOperationException("Failed to deserialize response.");
		}
		await HandleError(response, $"/User/{userId}/quizzes");
		throw new UnreachableException("This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.");
	}
	public async Task GetGetToken(string? returnUrl = null,Action<HttpRequestMessage>? configureRequest = null)	{
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/User/getToken?returnUrl={returnUrl ?? null}");
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			return;
		}
		await HandleError(response, $"/User/getToken");
		throw new UnreachableException("This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.");
	}
	public async Task GetLogout(string? returnUrl = null,Action<HttpRequestMessage>? configureRequest = null)	{
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/User/logout?returnUrl={returnUrl ?? null}");
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			return;
		}
		await HandleError(response, $"/User/logout");
		throw new UnreachableException("This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.");
	}
	public async Task<UserProfileReadModel>PatchUser(UserProfileReadModel userProfileReadModel,Action<HttpRequestMessage>? configureRequest = null, bool allowNullOrEmptyResponse = false, JsonSerializerOptions? jsonSerializerOptions = null)	{
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/User");
		httpRequest.Content = JsonContent.Create(userProfileReadModel, options: jsonSerializerOptions);
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<UserProfileReadModel>(options: jsonSerializerOptions);
			if (result is null && allowNullOrEmptyResponse)
			{
				return null!;
			}
			return result ?? throw new InvalidOperationException("Failed to deserialize response.");
		}
		await HandleError(response, $"/User");
		throw new UnreachableException("This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.");
	}
	public async Task<UserProfileReadModel>PostUpload(FileReadModel fileReadModel,Action<HttpRequestMessage>? configureRequest = null, bool allowNullOrEmptyResponse = false, JsonSerializerOptions? jsonSerializerOptions = null)	{
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/User/upload");
		httpRequest.Content = JsonContent.Create(fileReadModel, options: jsonSerializerOptions);
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<UserProfileReadModel>(options: jsonSerializerOptions);
			if (result is null && allowNullOrEmptyResponse)
			{
				return null!;
			}
			return result ?? throw new InvalidOperationException("Failed to deserialize response.");
		}
		await HandleError(response, $"/User/upload");
		throw new UnreachableException("This should never happen, as EnsureSuccessStatusCode should throw an exception if the status code is not successful.");
	}
	public async Task<UserProfileReadModel?>PatchColor(String color,Action<HttpRequestMessage>? configureRequest = null, bool allowNullOrEmptyResponse = false, JsonSerializerOptions? jsonSerializerOptions = null)	{
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/User/color");
		httpRequest.Content = JsonContent.Create(color, options: jsonSerializerOptions);
		configureRequest?.Invoke(httpRequest);
		HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
		if (response.IsSuccessStatusCode)
		{
			var result = await response.Content.ReadFromJsonAsync<UserProfileReadModel>(options: jsonSerializerOptions);
			if (result is null && allowNullOrEmptyResponse)
			{
				return null!;
			}
			return result ?? throw new InvalidOperationException("Failed to deserialize response.");
		}
		await HandleError(response, $"/User/color");
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
