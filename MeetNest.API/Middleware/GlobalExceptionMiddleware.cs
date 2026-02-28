using MeetNest.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace MeetNest.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AuthException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.Unauthorized,
                ex.Message);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.InternalServerError,
                "Something went wrong.");
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            message
        };

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}