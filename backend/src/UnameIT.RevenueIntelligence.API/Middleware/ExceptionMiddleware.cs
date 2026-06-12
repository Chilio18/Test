using System.Net;
using System.Text.Json;
using UnameIT.RevenueIntelligence.Domain.Exceptions;

namespace UnameIT.RevenueIntelligence.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
            await HandleExceptionAsync(ctx, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        ctx.Response.ContentType = "application/json";

        var (statusCode, response) = ex switch
        {
            NotFoundException nfe => (HttpStatusCode.NotFound, new ProblemResponse(
                "Not Found", nfe.Message, (int)HttpStatusCode.NotFound)),
            UnauthorizedException ue => (HttpStatusCode.Forbidden, new ProblemResponse(
                "Forbidden", ue.Message, (int)HttpStatusCode.Forbidden)),
            Domain.Exceptions.ValidationException ve => (HttpStatusCode.UnprocessableEntity,
                new ValidationProblemResponse("Validation Failed", ve.Errors)),
            TenantMismatchException tme => (HttpStatusCode.Forbidden, new ProblemResponse(
                "Forbidden", tme.Message, (int)HttpStatusCode.Forbidden)),
            _ => (HttpStatusCode.InternalServerError, new ProblemResponse(
                "Internal Server Error", "An unexpected error occurred.", (int)HttpStatusCode.InternalServerError))
        };

        ctx.Response.StatusCode = (int)statusCode;
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}

public record ProblemResponse(string Title, string Detail, int Status);
public record ValidationProblemResponse(string Title, IReadOnlyDictionary<string, string[]> Errors);
