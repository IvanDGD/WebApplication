 var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddControllersWithViews();

var app = builder.Build();
#region Main Task
//app.MapGet("/", async (IUserRepository repo, HttpContext context) =>
//{
//    var users = repo.GetAll();
//    context.Response.ContentType = "text/html;charset=utf8";
//    await context.Response.WriteAsync("<h1>Список пользователей</h1>");
//    await context.Response.WriteAsync("<a href='/create'>Добавить нового пользователя</a><br/><br/>");

//    if (!users.Any())
//    {
//        await context.Response.WriteAsync("Пользователей пока нет.");
//    }
//    else
//    {
//        foreach (var u in users)
//        {
//            await context.Response.WriteAsync($"ID: {u.Id} | {u.Name} | {u.Email} " +
//                $"<a href='/edit/{u.Id}'>Редактировать</a> | " +
//                $"<a href='/delete/{u.Id}'>Удалить</a><br/>");
//        }
//    }
//});

//app.MapGet("/create", async (HttpContext context) =>
//{
//    context.Response.ContentType = "text/html;charset=utf8";
//    await context.Response.WriteAsync(@"
//        <h1>Создать пользователя</h1>
//        <form method='post'>
//            Имя: <input name='Name' /><br/>
//            Email: <input name='Email' /><br/>
//            <button type='submit'>Создать</button>
//        </form>
//        <a href='/'>Назад</a>
//    ");
//});

//app.MapPost("/create", async (IUserRepository repo, HttpContext context) =>
//{
//    context.Response.ContentType = "text/html;charset=utf8";
//    var form = await context.Request.ReadFormAsync();
//    var name = form["Name"].ToString();
//    var email = form["Email"].ToString();

//    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
//    {
//        await context.Response.WriteAsync("Ошибка: имя и email обязательны.<br/><a href='/create'>Назад</a>");
//        return;
//    }

//    repo.Add(new User { Name = name, Email = email });
//    context.Response.Redirect("/");
//});

//app.MapGet("/edit/{id}", async (IUserRepository repo, HttpContext context, string id) =>
//{
//    context.Response.ContentType = "text/html;charset=utf8";
//    if (!int.TryParse(id, out int userId))
//    {
//        await context.Response.WriteAsync("Неверный ID.<br/><a href='/'>Назад</a>");
//        return;
//    }

//    var user = repo.Get(userId);
//    if (user == null)
//    {
//        await context.Response.WriteAsync("Пользователь не найден.<br/><a href='/'>Назад</a>");
//        return;
//    }

//    await context.Response.WriteAsync($@"
//        <h1>Редактировать пользователя {user.Id}</h1>
//        <form method='post'>
//            Имя: <input name='Name' value='{user.Name}' /><br/>
//            Email: <input name='Email' value='{user.Email}' /><br/>
//            <button type='submit'>Сохранить</button>
//        </form>
//        <a href='/'>Назад</a>
//    ");
//});

//app.MapPost("/edit/{id}", async (IUserRepository repo, HttpContext context, string id) =>
//{
//    context.Response.ContentType = "text/html;charset=utf8";
//    if (!int.TryParse(id, out int userId))
//    {
//        await context.Response.WriteAsync("Неверный ID.<br/><a href='/'>Назад</a>");
//        return;
//    }

//    var form = await context.Request.ReadFormAsync();
//    var name = form["Name"].ToString();
//    var email = form["Email"].ToString();

//    var success = repo.Update(new User { Id = userId, Name = name, Email = email });
//    if (!success)
//    {
//        await context.Response.WriteAsync("Пользователь не найден.<br/><a href='/'>Назад</a>");
//        return;
//    }

//    context.Response.Redirect("/");
//});

//app.MapGet("/delete/{id}", async (IUserRepository repo, HttpContext context, string id) =>
//{
//    context.Response.ContentType = "text/html;charset=utf8";
//    if (!int.TryParse(id, out int userId))
//    {
//        await context.Response.WriteAsync("Неверный ID.<br/><a href='/'>Назад</a>");
//        return;
//    }

//    var success = repo.Delete(userId);
//    if (!success)
//    {
//        await context.Response.WriteAsync("Пользователь не найден.<br/><a href='/'>Назад</a>");
//        return;
//    }

//    context.Response.Redirect("/");
//});
#endregion

#region Additional One
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/number") &&
        !context.Request.Query.ContainsKey("value"))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Query parameter 'value' is required"
        });
        return;
    }

    await next();
});

app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/number"))
    {
        await next();
        return;
    }

    var value = context.Request.Query["value"];

    if (!int.TryParse(value, out int number))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Value must be an integer"
        });
        return;
    }

    if (number < 1 || number > 100000)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Number must be from 1 to 100000"
        });
        return;
    }

    context.Items["number"] = number;
    await next();
});

app.MapGet("/number", (HttpContext context) =>
{
    int number = (int)context.Items["number"];
    string text = NumberToWords(number);

    return Results.Json(new
    {
        number,
        text
    });
});

app.Run();


static string NumberToWords(int number)
{
    if (number == 100000)
        return "one hundred thousand";

    string[] ones =
    {
        "", "one", "two", "three", "four", "five",
        "six", "seven", "eight", "nine", "ten",
        "eleven", "twelve", "thirteen", "fourteen",
        "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"
    };

    string[] tens =
    {
        "", "", "twenty", "thirty", "forty",
        "fifty", "sixty", "seventy", "eighty", "ninety"
    };

    string result = "";

    if (number >= 1000)
    {
        result += NumberToWords(number / 1000) + " thousand";
        number %= 1000;
        if (number > 0) result += " ";
    }

    if (number >= 100)
    {
        result += ones[number / 100] + " hundred";
        number %= 100;
        if (number > 0) result += " ";
    }

    if (number >= 20)
    {
        result += tens[number / 10];
        number %= 10;
        if (number > 0) result += " ";
    }

    if (number > 0 && number < 20)
    {
        result += ones[number];
    }

    return result.Trim();
}
#endregion

app.Run();

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public interface IUserRepository
{
    void Add(User user);
    bool Delete(int id);
    User? Get(int id);
    bool Update(User user);
    List<User> GetAll();
}

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public void Add(User user)
    {
        user.Id = _nextId++;
        _users.Add(user);
    }

    public bool Delete(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null) return false;
        _users.Remove(user);
        return true;
    }

    public User? Get(int id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public bool Update(User user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing == null) return false;
        existing.Name = user.Name;
        existing.Email = user.Email;
        return true;
    }

    public List<User> GetAll()
    {
        return _users;
    }
}