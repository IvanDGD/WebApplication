var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<SingletonService>();
builder.Services.AddScoped<ScopedService>();
builder.Services.AddTransient<TransientService>();

var app = builder.Build();

app.UseMiddleware<LifecycleMiddleware>();

app.Run();
public interface IGuidService
{
    Guid Id { get; }
}

public class SingletonService : IGuidService
{
    public Guid Id { get; } = Guid.NewGuid();
}

public class ScopedService : IGuidService
{
    public Guid Id { get; } = Guid.NewGuid();
}

public class TransientService : IGuidService
{
    public Guid Id { get; } = Guid.NewGuid();
}
public class LifecycleMiddleware
{
    readonly RequestDelegate next;
    readonly SingletonService singletonConstructor;

    public LifecycleMiddleware(RequestDelegate next, SingletonService singleton)
    {
        this.next = next;
        this.singletonConstructor = singleton;
    }
    public async Task InvokeAsync(HttpContext context, ScopedService scopedFromParam, TransientService transientFromParam)
    {
        context.Response.ContentType = "text/html; charset=utf-8";

        var singletonFromRequest = context.RequestServices.GetService<SingletonService>();
        var scopedFromRequest = context.RequestServices.GetService<ScopedService>();
        var transientFromRequest = context.RequestServices.GetService<TransientService>();

        string html = $"""
            <b>Singleton (constructor):</b> {singletonConstructor.Id}<br>
            Singleton (RequestServices): {singletonFromRequest.Id}<br><br>

            <b>Scoped (param):</b> {scopedFromParam.Id}<br>
            Scoped (RequestServices): {scopedFromRequest.Id}<br><br>

            <b>Transient (param):</b> {transientFromParam.Id}<br>
            Transient (RequestServices): {transientFromRequest.Id}<br>
            """;
        await context.Response.WriteAsync(html);
    }
}