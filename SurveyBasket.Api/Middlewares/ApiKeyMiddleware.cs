namespace SurveyBasket.Middlewares;

public class ApiKeyMiddleware(RequestDelegate next)
{
    //private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "x-api-key";

   
    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("API Key is missing.");
            return;
        }

        var apiKey = configuration.GetValue<string>("ApiKey"); // Retrieve from configuration

        if (!apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = 403; // Forbidden
            await context.Response.WriteAsync("Invalid API Key.");
            return;
        }

        await next(context);
    }
}