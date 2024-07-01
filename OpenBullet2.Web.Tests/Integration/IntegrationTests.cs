using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Models.Errors;
using OpenBullet2.Web.Tests.Utils;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class IntegrationTests : IDisposable
{
    private readonly IServiceScope _serviceScope;
    private readonly ITestOutputHelper _testOutputHelper;
    
    protected IntegrationTests(ITestOutputHelper testOutputHelper)
    {
        Factory = new WebApplicationFactory<Program>();
        _testOutputHelper = testOutputHelper;
        
        var enumConverter = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase);
        JsonSerializerOptions.Converters.Add(enumConverter);
        JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        JsonSerializerOptions.IncludeFields = true;
        
        // Override the user data folder and connection string
        // to avoid conflicts with other tests
        UserDataFolder = Path.Combine(Path.GetTempPath(), $"OB2_UserData_{Guid.NewGuid():N}");
        Environment.SetEnvironmentVariable("Settings__UserDataFolder", UserDataFolder);
        
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection",
            $"Data Source={UserDataFolder}/OpenBullet.db;");
        
        _serviceScope = Factory.Services.CreateScope();
    }
    
    protected WebApplicationFactory<Program> Factory { get; }
    protected JsonSerializerOptions JsonSerializerOptions { get; } = new();
    protected string UserDataFolder { get; }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected void RequireLogin()
    {
        var obSettingsService = GetRequiredService<OpenBulletSettingsService>();
        obSettingsService.Settings.SecuritySettings.RequireAdminLogin = true;
    }
    
    /// <summary>
    /// Sets the Authorization header of the given HttpClient to a JWT
    /// with the claims of the given guest user.
    /// </summary>
    protected void ImpersonateGuest(HttpClient client, GuestEntity guest)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, guest.Id.ToString(), ClaimValueTypes.Integer),
            new Claim(ClaimTypes.Name, guest.Username),
            new Claim(ClaimTypes.Role, "Guest"),
            new Claim("IPAtLogin", "::1")
        };
        
        var authService = GetRequiredService<IAuthTokenService>();
        var token = authService.GenerateToken(claims, TimeSpan.FromHours(6));
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
    
    protected T GetRequiredService<T>() where T : notnull
    {
        return _serviceScope.ServiceProvider.GetRequiredService<T>();
    }
    
    protected async Task<ApiErrorResponse?> SendAsync(HttpClient client, Uri url, object? dto, HttpMethod method)
    {
        var json = dto is null
            ? null
            : JsonContent.Create(dto, MediaTypeHeaderValue.Parse("application/json"), JsonSerializerOptions);
        
        var request = new HttpRequestMessage(method, url) { Content = json };
        var response = await client.SendAsync(request);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            _testOutputHelper.WriteLine($"API status code: {response.StatusCode}");
            _testOutputHelper.WriteLine($"API response: {jsonResponse}");
            
            try
            {
                return new ApiErrorResponse
                {
                    Content = JsonSerializer.Deserialize<ApiError>(jsonResponse, JsonSerializerOptions)!,
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
    
    protected async Task<Result<T, ApiErrorResponse>> SendJsonAsync<T>(HttpClient client, Uri url, object? dto,
        HttpMethod method)
    {
        var json = dto is null
            ? null
            : JsonContent.Create(dto, MediaTypeHeaderValue.Parse("application/json"), JsonSerializerOptions);
        
        var request = new HttpRequestMessage(method, url) { Content = json };
        var response = await client.SendAsync(request);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            _testOutputHelper.WriteLine($"API status code: {response.StatusCode}");
            _testOutputHelper.WriteLine($"API response: {jsonResponse}");
            
            try
            {
                return new ApiErrorResponse
                {
                    Content = JsonSerializer.Deserialize<ApiError>(jsonResponse, JsonSerializerOptions)!,
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
        
        return JsonSerializer.Deserialize<T>(jsonResponse, JsonSerializerOptions)!;
    }
    
    protected Task<Result<T, ApiErrorResponse>> GetJsonAsync<T>(HttpClient client, string url)
    {
        return GetJsonAsync<T>(client, new Uri(url, UriKind.Relative));
    }
    
    protected async Task<Result<T, ApiErrorResponse>> GetJsonAsync<T>(HttpClient client, Uri url)
    {
        return await SendJsonAsync<T>(client, url, null, HttpMethod.Get);
    }
    
    protected async Task<Result<T, ApiErrorResponse>> PostJsonAsync<T>(HttpClient client, string url, object dto)
    {
        return await SendJsonAsync<T>(client, new Uri(url, UriKind.Relative), dto, HttpMethod.Post);
    }
    
    protected async Task<Result<T, ApiErrorResponse>> PostJsonAsync<T>(HttpClient client, Uri url, object dto)
    {
        return await SendJsonAsync<T>(client, url, dto, HttpMethod.Post);
    }
    
    protected async Task<Result<T, ApiErrorResponse>> PutJsonAsync<T>(HttpClient client, string url, object dto)
    {
        return await SendJsonAsync<T>(client, new Uri(url, UriKind.Relative), dto, HttpMethod.Put);
    }
    
    protected async Task<Result<T, ApiErrorResponse>> PatchJsonAsync<T>(HttpClient client, string url, object dto)
    {
        return await SendJsonAsync<T>(client, new Uri(url, UriKind.Relative), dto, HttpMethod.Patch);
    }
    
    protected Task<Result<T, ApiErrorResponse>> DeleteJsonAsync<T>(HttpClient client, string url)
    {
        return DeleteJsonAsync<T>(client, new Uri(url, UriKind.Relative));
    }
    
    protected async Task<Result<T, ApiErrorResponse>> DeleteJsonAsync<T>(HttpClient client, Uri url)
    {
        return await SendJsonAsync<T>(client, url, null, HttpMethod.Delete);
    }
    
    protected async Task<ApiErrorResponse?> PostAsync(HttpClient client, string url, object? dto)
    {
        return await SendAsync(client, new Uri(url, UriKind.Relative), dto, HttpMethod.Post);
    }
    
    protected async Task<ApiErrorResponse?> PostAsync(HttpClient client, Uri url, object? dto)
    {
        return await SendAsync(client, url, dto, HttpMethod.Post);
    }
    
    protected async Task<ApiErrorResponse?> PatchAsync(HttpClient client, string url, object? dto)
    {
        return await SendAsync(client, new Uri(url, UriKind.Relative), dto, HttpMethod.Patch);
    }
    
    protected async Task<ApiErrorResponse?> PatchAsync(HttpClient client, Uri url, object? dto)
    {
        return await SendAsync(client, url, dto, HttpMethod.Post);
    }
    
    protected async Task<ApiErrorResponse?> DeleteAsync(HttpClient client, Uri url)
    {
        return await SendAsync(client, url, null, HttpMethod.Delete);
    }
    
    protected async Task<ApiErrorResponse?> DeleteAsync(HttpClient client, string url)
    {
        return await SendAsync(client, new Uri(url, UriKind.Relative), null, HttpMethod.Delete);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        _serviceScope.Dispose();
        Factory.Dispose();
    }
    
    ~IntegrationTests()
    {
        Dispose(false);
    }
}
