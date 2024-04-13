using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Models.Errors;
using System.Net;
using System.Text.Json;
using FluentValidation;

namespace OpenBullet2.Web.Middleware;

internal class ExceptionMiddleware
{
    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;

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
        catch (ValidationException ex)
        {
            await RespondAsync(context,
                new ApiError(ErrorCode.ValidationError, ex.Message),
                HttpStatusCode.BadRequest);
        }
        catch (UnauthorizedException ex)
        {
            await RespondAsync(context,
                new ApiError(ex.ErrorCode, ex.Message),
                HttpStatusCode.Unauthorized);
        }
        catch (ForbiddenException ex)
        {
            await RespondAsync(context,
                new ApiError(ex.ErrorCode, ex.Message),
                HttpStatusCode.Forbidden);
        }
        catch (ResourceNotFoundException ex)
        {
            await RespondAsync(context,
                new ApiError(ex.ErrorCode, ex.Message),
                HttpStatusCode.NotFound);
        }
        catch (ApiException ex)
        {
            await RespondAsync(context,
                new ApiError(ex.ErrorCode, ex.Message, ex.StackTrace?.Trim()),
                HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generic exception");
            await RespondAsync(context,
                new ApiError(ErrorCode.InternalServerError, ex.Message,
                    ex.StackTrace?.Trim()), HttpStatusCode.InternalServerError);
        }
    }

    private async Task RespondAsync(HttpContext context, ApiError error,
        HttpStatusCode statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(error, _jsonSerializerOptions)
        );
    }
}
