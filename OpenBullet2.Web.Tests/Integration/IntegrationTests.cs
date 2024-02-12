using Microsoft.AspNetCore.Mvc.Testing;
using OpenBullet2.Web.Models.Errors;
using OpenBullet2.Web.Tests.Utils;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new();
    protected WebApplicationFactory<Program> Factory { get; }
    
    protected IntegrationTests(WebApplicationFactory<Program> factory,
        ITestOutputHelper testOutputHelper)
    {
        Factory = factory;
        _testOutputHelper = testOutputHelper;
        
        var enumConverter = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase);
        _jsonSerializerOptions.Converters.Add(enumConverter);
        _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        
        // Override the user data folder and connection string
        // to avoid conflicts with the main application and other tests
        var userDataFolder = Path.Combine(Path.GetTempPath(), $"OB2_UserData_{Guid.NewGuid():N}");
        Environment.SetEnvironmentVariable("Settings__UserDataFolder", userDataFolder);
        
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection",
            $"Data Source={userDataFolder}/OpenBullet.db;");
    }
    
    protected Task<Result<T, ApiErrorResponse>> GetJsonAsync<T>(HttpClient client, string url)
        => GetJsonAsync<T>(client, new Uri(url, UriKind.Relative));
 
    protected async Task<Result<T, ApiErrorResponse>> GetJsonAsync<T>(HttpClient client, Uri url)
    {
        var response = await client.GetAsync(url);
        var jsonResponse = await response.Content.ReadAsStringAsync();
 
        if (!response.IsSuccessStatusCode)
        {
            _testOutputHelper.WriteLine($"API status code: {response.StatusCode}");
            _testOutputHelper.WriteLine($"API response: {jsonResponse}");
 
            try
            {
                return new ApiErrorResponse
                {
                    Content = JsonSerializer.Deserialize<ApiError>(jsonResponse, _jsonSerializerOptions)!,
                    Response = response
                }!;
            }
            catch (JsonException)
            {
                return new ApiErrorResponse
                {
                    Response = response
                }!;
            }
        }
 
        return JsonSerializer.Deserialize<T>(jsonResponse, _jsonSerializerOptions)!;
    }
 
    protected async Task<ApiErrorResponse?> PostJsonAsync(HttpClient client, string url, object dto)
        => await PostJsonAsync(client, new Uri(url, UriKind.Relative), dto);
 
    protected async Task<ApiErrorResponse?> PostJsonAsync(HttpClient client, Uri url, object dto)
    {
        var json = JsonContent.Create(dto, MediaTypeHeaderValue.Parse("application/json"), _jsonSerializerOptions);
        var response = await client.PostAsync(url, json);
        var jsonResponse = await response.Content.ReadAsStringAsync();
 
        if (!response.IsSuccessStatusCode)
        {
            _testOutputHelper.WriteLine($"API status code: {response.StatusCode}");
            _testOutputHelper.WriteLine($"API response: {jsonResponse}");
 
            try
            {
                return new ApiErrorResponse
                {
                    Content = JsonSerializer.Deserialize<ApiError>(jsonResponse, _jsonSerializerOptions)!,
                    Response = response
                };
            }
            catch (JsonException)
            {
                return new ApiErrorResponse
                {
                    Response = response
                };
            }
        }
 
        return null;
    }
 
    protected async Task<Result<T, ApiErrorResponse>> PostJsonAsync<T>(HttpClient client, string url, object dto)
        => await PostJsonAsync<T>(client, new Uri(url, UriKind.Relative), dto);
 
    protected async Task<Result<T, ApiErrorResponse>> PostJsonAsync<T>(HttpClient client, Uri url, object dto)
    {
        var json = JsonContent.Create(dto, MediaTypeHeaderValue.Parse("application/json"), _jsonSerializerOptions);
        var response = await client.PostAsync(url, json);
        var jsonResponse = await response.Content.ReadAsStringAsync();
 
        if (!response.IsSuccessStatusCode)
        {
            _testOutputHelper.WriteLine($"API status code: {response.StatusCode}");
            _testOutputHelper.WriteLine($"API response: {jsonResponse}");
 
            try
            {
                return new ApiErrorResponse
                {
                    Content = JsonSerializer.Deserialize<ApiError>(jsonResponse, _jsonSerializerOptions)!,
                    Response = response
                }!;
            }
            catch (JsonException)
            {
                return new ApiErrorResponse
                {
                    Response = response
                }!;
            }
        }
 
        return JsonSerializer.Deserialize<T>(jsonResponse, _jsonSerializerOptions)!;
    }
 
    protected async Task<Result<T, ApiErrorResponse>> PutJsonAsync<T>(HttpClient client, string url, object dto)
        => await PutJsonAsync<T>(client, new Uri(url, UriKind.Relative), dto);
 
    protected async Task<Result<T, ApiErrorResponse>> PutJsonAsync<T>(HttpClient client, Uri url, object dto)
    {
        var json = JsonContent.Create(dto, MediaTypeHeaderValue.Parse("application/json"), _jsonSerializerOptions);
        var response = await client.PutAsync(url, json);
        var jsonResponse = await response.Content.ReadAsStringAsync();
 
        if (!response.IsSuccessStatusCode)
        {
            _testOutputHelper.WriteLine($"API status code: {response.StatusCode}");
            _testOutputHelper.WriteLine($"API response: {jsonResponse}");
 
            try
            {
                return new ApiErrorResponse
                {
                    Content = JsonSerializer.Deserialize<ApiError>(jsonResponse, _jsonSerializerOptions)!,
                    Response = response
                }!;
            }
            catch (JsonException)
            {
                return new ApiErrorResponse
                {
                    Response = response
                }!;
            }
        }
 
        return JsonSerializer.Deserialize<T>(jsonResponse, _jsonSerializerOptions)!;
    }
 
    protected async Task<ApiErrorResponse?> DeleteAsync(HttpClient client, string url)
        => await DeleteAsync(client, new Uri(url, UriKind.Relative));
 
    protected async Task<ApiErrorResponse?> DeleteAsync(HttpClient client, Uri url)
    {
        var response = await client.DeleteAsync(url);
        var jsonResponse = await response.Content.ReadAsStringAsync();
 
        if (!response.IsSuccessStatusCode)
        {
            _testOutputHelper.WriteLine($"API status code: {response.StatusCode}");
            _testOutputHelper.WriteLine($"API response: {jsonResponse}");
 
            try
            {
                return new ApiErrorResponse
                {
                    Content = JsonSerializer.Deserialize<ApiError>(jsonResponse, _jsonSerializerOptions)!,
                    Response = response
                };
            }
            catch (JsonException)
            {
                return new ApiErrorResponse
                {
                    Response = response
                };
            }
        }
 
        return null;
    }
}
