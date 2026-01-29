var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

#region Main Task
//const int secretNumber = 3;
//const string secretKey = "letmein";

//app.Run(async context =>
//{
//    var request = context.Request;
//    var response = context.Response;

//    if (request.Path != "/guess")
//    {
//        response.StatusCode = 404;
//        return;
//    }
//    string key = request.Headers["X-Secret-Key"];
//    if (key != secretKey)
//    {
//        response.StatusCode = 401;
//        return;
//    }
//    string guessValue = request.Query["guess"];
//    if (!int.TryParse(guessValue, out int guess))
//    {
//        response.StatusCode = 400;
//        return;
//    }
//    if (guess == secretNumber)
//    {
//        response.StatusCode = 200;
//        await response.WriteAsync("Correct!");
//    }
//    else
//    {
//        response.StatusCode = 400;
//        await response.WriteAsync("Wrong!");
//    }
//});
#endregion

#region Additional Task 1
app.Run(async context =>
{
    var request = context.Request;
    var response = context.Response;
    var path = request.Path;


    if (path.Value.Contains("/error"))
    {
        response.StatusCode = 500;
        return;
    }

    response.StatusCode = 200;
    response.ContentType = "application/json";

    var result = new
    {
        Path = request.Path.Value,
        Query = request.Query.ToDictionary(q => q.Key, q => q.Value.ToString()),
        Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
    };

    await response.WriteAsJsonAsync(result);
});
#endregion

app.Run();