using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

#region Main Task
//var currencies = new[]
//{
//    new { code = "USD", name = "US Dollar" },
//    new { code = "EUR", name = "Euro" },
//    new { code = "UAH", name = "Hryvnia" }
//};

//var rates = new Dictionary<string, decimal>
//{
//    { "USD_EUR", 0.92m },
//    { "EUR_USD", 1.09m },
//    { "USD_UAH", 38.5m },
//    { "UAH_USD", 0.026m },
//    { "EUR_UAH", 41.8m },
//    { "UAH_EUR", 0.024m }
//};

//app.MapGet("/", async (context) =>
//{
//    context.Response.ContentType = "text/html; charset=utf-8";

//    var html = """
//            <h2>Welcome on Currency Web API!</h2>
//            <h4>Our commands</h4>
//            <a href="/currencies">/currencies<a/></br>
//            <a href="/exchangeRate?from=&to=">/exchangeRate?from=&to=<a/></br>
//            <a href="/convertCurrency?from=&to=&amount=">/convertCurrency?from=&to=&amount=<a/></br>
//    """;

//    await context.Response.WriteAsync(html);
//});

//app.Map("/currencies", async context =>
//{
//    context.Response.ContentType = "text/html; charset=utf-8";

//    await context.Response.WriteAsJsonAsync(currencies);
//});


//app.Map("/exchangeRate", async context =>
//{
//    var from = context.Request.Query["from"].ToString();
//    var to = context.Request.Query["to"].ToString();

//    var key = from + "_" + to;

//    if (!rates.ContainsKey(key))
//    {
//        context.Response.StatusCode = 404;
//        await context.Response.WriteAsJsonAsync(new { error = "Rate not found" });
//        return;
//    }

//    await context.Response.WriteAsJsonAsync(new
//    {
//        from,
//        to,
//        rate = rates[key]
//    });
//});


//app.Map("/convertCurrency", async context =>
//{
//    var from = context.Request.Query["from"].ToString();
//    var to = context.Request.Query["to"].ToString();
//    var amount = decimal.Parse(context.Request.Query["amount"]);

//    var key = from + "_" + to;

//    if (!rates.ContainsKey(key))
//    {
//        context.Response.StatusCode = 400;
//        await context.Response.WriteAsJsonAsync(new { error = "Conversion error" });
//        return;
//    }

//    await context.Response.WriteAsJsonAsync(new
//    {
//        from,
//        to,
//        amount,
//        result = amount * rates[key]
//    });
//});
#endregion

#region Additional First
//app.MapWhen(
//    context =>
//        context.Request.Path == "/" ||
//        context.Request.Path == "/about" ||
//        context.Request.Path == "/services" ||
//        context.Request.Path == "/contacts" ||
//        context.Request.Path == "/help",
//    app =>
//    {
//        app.Run(async context =>
//        {
//            context.Response.ContentType = "text/html; charset=utf-8";

//            var path = context.Request.Path.Value;

//            if (path == "/")
//            {
//                await context.Response.WriteAsync("<h1>Главная</h1>");
//            }
//            else if (path == "/about")
//            {
//                await context.Response.WriteAsync("<h1>О нас</h1>");
//            }
//            else if (path == "/services")
//            {
//                await context.Response.WriteAsync("<h1>Услуги</h1>");
//            }
//            else if (path == "/contacts")
//            {
//                await context.Response.WriteAsync("<h1>Контакты</h1>");
//            }
//            else if (path == "/help")
//            {
//                await context.Response.WriteAsync("<h1>Помощь</h1>");
//            }
//        });
//    }
//);

//app.Run(async context =>
//{
//    context.Response.StatusCode = 404;
//    await context.Response.WriteAsync("<h1>404</h1>");
//});
#endregion

#region Additional Second
app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();

    await next();

    stopwatch.Stop();

    Console.WriteLine(
        $"Method: {context.Request.Method}, " +
        $"Path: {context.Request.Path}, " +
        $"Time: {stopwatch.ElapsedMilliseconds} ms"
    );
});

app.Use(async (context, next) =>
{
    const long maxSize = 1 * 1024 * 1024;

    if (context.Request.ContentLength.HasValue &&
        context.Request.ContentLength.Value > maxSize)
    {
        context.Response.StatusCode = 413;
        await context.Response.WriteAsync("Payload Too Large");
        return;
    }

    await next();
});


app.Run(async context =>
{
    long size = 0;
    context.Response.ContentType = "text/html;charset=utf8";
    if (context.Request.ContentLength.HasValue)
    {
        size = context.Request.ContentLength.Value;
    }
    
    await context.Response.WriteAsync(
        $"успешно обработано, размер: {size} байт"
    );
});
#endregion

app.Run();
