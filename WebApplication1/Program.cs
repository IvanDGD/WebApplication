using Microsoft.Data.SqlClient;
using System.Text;

var builder = WebApplication.CreateBuilder();
var app = builder.Build();
string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=Testdb;Trusted_Connection=True;TrustServerCertificate=True";

app.Run(async (context) =>
{
    var request = context.Request;
    var response = context.Response;
    response.ContentType = "text/html; charset=utf-8";

    if (request.Path == "/")
    {
        response.ContentType = "text/html; charset=utf-8";
        var sb = new StringBuilder();
        sb.Append("<table class='table'>");

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            string sql = @"
            SELECT q.Id, q.Title, q.Description, COUNT(ques.Id) AS QuestionCount
            FROM Quiz q
            LEFT JOIN Questions ques ON q.Id = ques.QuizId
            GROUP BY q.Id, q.Title, q.Description
        ";

            using SqlCommand command = new SqlCommand(sql, connection);
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int id = reader.GetInt32(0);
                string title = reader.GetString(1);
                string description = reader.GetString(2);
                int questionCount = reader.GetInt32(3);

                sb.Append($"""
                <tr>
                    <td>
                        <a href="/quiz/{id}">
                            <h5>{title}</h5>
                            <div style="display:flex; flex-direction: row;">{description} <p style="margin-left:15px;">{questionCount}</p></div>
                        </a>
                    </td>
                </tr>
            """);
            }
        }

        sb.Append("</table>");
        await response.WriteAsync(GenerateHtmlPage(sb.ToString(), "Îïðîñû"));
        return;
    }
    else if (request.Path.StartsWithSegments("/quiz"))
    {
        var parts = request.Path.Value!.Split('/');
        if (parts.Length == 3 && int.TryParse(parts[2], out int surveyId))
        {
            if (request.Method == "GET")
            {
                var questions = new List<(int Id, string Text)>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(
                        "SELECT Id, QuestionText FROM Questions WHERE QuizId = @QuizId",
                        connection
                    );
                    command.Parameters.AddWithValue("@QuizId", surveyId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            questions.Add((reader.GetInt32(0), reader.GetString(1)));
                        }
                    }
                }

                var sb = new StringBuilder();
                sb.Append($"""
                <form method="post" action="/quiz/{surveyId}">
                    <input type="hidden" name="surveyId" value="{surveyId}" />

                    <div class="mb-3">
                        <label>ÔÈÎ</label>
                        <input name="fullName" class="form-control" required />
                    </div>

                    <div class="mb-3">
                        <label>Òåëåôîí</label>
                        <input name="phone" class="form-control" required />
                    </div>
            """);

                int qNumber = 1;
                foreach (var q in questions)
                {
                    sb.Append($"""
                    <div class="mb-3">
                        <label>Q{qNumber}: {q.Text}</label>
                        <input name="q_{q.Id}" class="form-control" required />
                    </div>
                """);
                    qNumber++;
                }

                sb.Append("<button class='btn btn-success'>Îòïðàâèòü</button></form>");

                await context.Response.WriteAsync(GenerateHtmlPage(sb.ToString(), "Ïðîõîæäåíèå îïðîñà"));
                return;
            }
            else if (request.Method == "POST")
            {
                var form = await request.ReadFormAsync();
                string fullName = form["fullName"];
                string phone = form["phone"];

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(
                        "INSERT INTO Users (SurveyId, FullName, Phone, CreatedAt) VALUES (@QuizId, @FullName, @Phone, GETDATE())",
                        connection
                    );
                    command.Parameters.AddWithValue("@QuizId", surveyId);
                    command.Parameters.AddWithValue("@FullName", fullName);
                    command.Parameters.AddWithValue("@Phone", phone);
                    command.ExecuteNonQuery();
                }

                response.Redirect("/");
                return;
            }
        }
    }
    else if (request.Path == "/admin")
    {
        var name = context.Request.Query["name"];
        var password = context.Request.Query["password"];

        if (name == "admin" && password == "admin")
        {
            var sb = new StringBuilder("<table class='table'>");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand countCmd = new SqlCommand("SELECT COUNT(*) FROM Users", connection);
                int count = (int)countCmd.ExecuteScalar();

                for (int i = 1; i <= count; i++)
                {
                    SqlCommand surveyCmd = new SqlCommand($"SELECT SurveyId FROM Users WHERE Id = {i}", connection);
                    SqlCommand fullNameCmd = new SqlCommand($"SELECT FullName FROM Users WHERE Id = {i}", connection);
                    SqlCommand phoneCmd = new SqlCommand($"SELECT Phone FROM Users WHERE Id = {i}", connection);
                    SqlCommand createdCmd = new SqlCommand($"SELECT FORMAT(CreatedAt, 'yyyy-MM-dd HH:mm:ss') FROM Users WHERE Id = {i}", connection);

                    var surveyId = surveyCmd.ExecuteScalar();
                    var fullName = fullNameCmd.ExecuteScalar();
                    var phone = phoneCmd.ExecuteScalar();
                    var createdAt = createdCmd.ExecuteScalar();

                    if (surveyId != null)
                    {
                        sb.Append($"""
                        <tr>
                            <td>Îïðîñ #{surveyId}</td>
                            <td>{fullName}</td>
                            <td>{phone}</td>
                            <td>{createdAt}</td>
                        </tr>
                    """);
                    }
                }
            }

            sb.Append("</table>");
            await response.WriteAsync(GenerateHtmlPage(sb.ToString(), "Ðåçóëüòàòû"));
        }
    }

    else
    {
        response.StatusCode = 404;
        await response.WriteAsync("Page Not Found");
    }
});


app.Run();

static string GenerateHtmlPage(string body, string header)
{
    string html = $"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8" />
            <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha3/dist/css/bootstrap.min.css" rel="stylesheet" 
            integrity="sha384-KK94CHFLLe+nY2dmCWGMq91rCGa5gtU4mk92HdvYe+M/SXH301p5ILy+dN9+nJOZ" crossorigin="anonymous">
            <title>{header}</title>
        </head>
        <body>
        <div class="container">
        <h2 class="d-flex justify-content-center">{header}</h2>
        <div class="mt-5"></div>
        {body}
            <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha3/dist/js/bootstrap.bundle.min.js"
            integrity="sha384-ENjdO4Dr2bkBIFxQpeoTz1HIcje39Wm4jDKdf19U8gI4ddQ3GYNS7NTKfAdVQSZe" crossorigin="anonymous"></script>
        </div>
        </body>
        </html>
        """;
    return html;
}
