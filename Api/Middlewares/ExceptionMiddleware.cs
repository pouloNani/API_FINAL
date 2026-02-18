// Api/Middleware/ExceptionMiddleware.cs
using Core.POCO;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context); // exécute la requête normalement
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            var error = new AppError(500, ex.Message);
            await context.Response.WriteAsJsonAsync(error);
        }
    }
}