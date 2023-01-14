using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Models.Errors;
using System.Net;
using System.Text.Json;

namespace OpenBullet2.Web.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy= JsonNamingPolicy.CamelCase
    };

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"Unauthorized request: {ex.Message}");
            await Respond(context,
                new ApiError((int)ErrorCode.UNAUTHORIZED, ex.Message),
                HttpStatusCode.Unauthorized);
        }
        catch (ApiException ex)
        {
            _logger.LogWarning($"Request failed with managed exception: {ex}");
            await Respond(context,
                new ApiError((int)ex.ErrorCode, ex.ToString()),
                HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await Respond(context,
                new ApiError((int)ErrorCode.INTERNAL_SERVER_ERROR, ex.Message, ex.StackTrace?.Trim()),
                HttpStatusCode.InternalServerError);
        }
    }

    private async Task Respond(HttpContext context, ApiError error,
        HttpStatusCode statusCode)
    {
        context.Response.ContentType= "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(error, _jsonSerializerOptions)
        );
    }
}
