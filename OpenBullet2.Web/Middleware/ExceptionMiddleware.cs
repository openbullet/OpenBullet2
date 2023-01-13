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
            await LogAndRespond(context, ex,
                HttpStatusCode.Unauthorized, ErrorCode.UNAUTHORIZED);
        }
        catch (ApiException ex)
        {
            await LogAndRespond(context, ex,
                HttpStatusCode.NotFound, ex.ErrorCode);
        }
        catch (Exception ex)
        {
            await LogAndRespond(context, ex,
                HttpStatusCode.InternalServerError,
                ErrorCode.INTERNAL_SERVER_ERROR);
        }
    }

    private async Task LogAndRespond(HttpContext context, Exception ex,
        HttpStatusCode statusCode, ErrorCode errorCode)
    {
        _logger.LogError(ex, ex.Message);

        context.Response.ContentType= "application/json";
        context.Response.StatusCode = (int)statusCode;

        var error = new ApiError((int)errorCode, ex.Message, 
            ex.StackTrace?.ToString().Trim());

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(error, _jsonSerializerOptions)
        );
    }
}
