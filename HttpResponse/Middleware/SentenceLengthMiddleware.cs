public class SentenceLengthMiddleware
{
    readonly RequestDelegate next;
    public SentenceLengthMiddleware(RequestDelegate next)
    {
        this.next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        string? value = context.Request.Query["value"];
        if (value != null)
        {
            await context.Response.WriteAsync($"Value length: {value.Length}");
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
}