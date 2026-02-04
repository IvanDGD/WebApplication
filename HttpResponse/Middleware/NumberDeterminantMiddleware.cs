using Humanizer;

public class NumberDeterminantMiddleware
{
    readonly RequestDelegate next;
    public NumberDeterminantMiddleware(RequestDelegate next)
    {
        this.next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        if (int.TryParse(context.Request.Query["value"], out int result))
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(result.ToWords());
        }
        else
        {
            await next.Invoke(context);
        }
    }
}