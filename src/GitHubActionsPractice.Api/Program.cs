using GitHubActionsPractice.Api.Contracts;
using GitHubActionsPractice.Api.Data;
using GitHubActionsPractice.Api.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = ResolveConnectionString(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required."),
    builder.Environment.ContentRootPath);

builder.Services.AddDbContext<TodoDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddHealthChecks().AddDbContextCheck<TodoDbContext>();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    await database.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapGet("/debug/environment", (HttpContext context) =>
    {
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers.Pragma = "no-cache";
        var environment = Environment.GetEnvironmentVariables()
            .Cast<System.Collections.DictionaryEntry>()
            .ToDictionary(entry => entry.Key.ToString()!, entry => entry.Value?.ToString(), StringComparer.Ordinal);
        return Results.Json(environment.OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .ToDictionary(entry => entry.Key, entry => entry.Value));
    });
}

var todos = app.MapGroup("/api/todos");

todos.MapGet("/", async (TodoDbContext database, CancellationToken cancellationToken) =>
    Results.Ok(await database.Todos.AsNoTracking().OrderBy(todo => todo.Id)
        .Select(todo => new TodoResponse(todo.Id, todo.Title, todo.IsCompleted, todo.CreatedAtUtc))
        .ToListAsync(cancellationToken)));

todos.MapGet("/{id:int}", async (int id, TodoDbContext database, CancellationToken cancellationToken) =>
{
    var todo = await database.Todos.AsNoTracking().SingleOrDefaultAsync(todo => todo.Id == id, cancellationToken);
    return todo is null ? Results.NotFound() : Results.Ok(ToResponse(todo));
});

todos.MapPost("/", async (CreateTodoRequest request, TodoDbContext database, CancellationToken cancellationToken) =>
{
    var titleError = ValidateTitle(request.Title, out var title);
    if (titleError is not null) return titleError;

    var todo = new TodoItem { Title = title!, IsCompleted = false, CreatedAtUtc = DateTime.UtcNow };
    database.Todos.Add(todo);
    await database.SaveChangesAsync(cancellationToken);
    return Results.Created($"/api/todos/{todo.Id}", ToResponse(todo));
});

todos.MapPut("/{id:int}", async (int id, UpdateTodoRequest request, TodoDbContext database, CancellationToken cancellationToken) =>
{
    var titleError = ValidateTitle(request.Title, out var title);
    if (titleError is not null) return titleError;

    var todo = await database.Todos.SingleOrDefaultAsync(todo => todo.Id == id, cancellationToken);
    if (todo is null) return Results.NotFound();

    todo.Title = title!;
    todo.IsCompleted = request.IsCompleted;
    await database.SaveChangesAsync(cancellationToken);
    return Results.Ok(ToResponse(todo));
});

todos.MapDelete("/{id:int}", async (int id, TodoDbContext database, CancellationToken cancellationToken) =>
{
    var todo = await database.Todos.SingleOrDefaultAsync(todo => todo.Id == id, cancellationToken);
    if (todo is null) return Results.NotFound();

    database.Todos.Remove(todo);
    await database.SaveChangesAsync(cancellationToken);
    return Results.NoContent();
});

app.MapHealthChecks("/health");
app.Run();

static string ResolveConnectionString(string connectionString, string contentRootPath)
{
    var builder = new SqliteConnectionStringBuilder(connectionString);
    if (!string.IsNullOrWhiteSpace(builder.DataSource) && !Path.IsPathFullyQualified(builder.DataSource))
    {
        builder.DataSource = Path.GetFullPath(builder.DataSource, contentRootPath);
    }

    var directory = Path.GetDirectoryName(builder.DataSource);
    if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
    return builder.ConnectionString;
}

static IResult? ValidateTitle(string? title, out string? trimmedTitle)
{
    trimmedTitle = title?.Trim();
    return string.IsNullOrWhiteSpace(trimmedTitle) || trimmedTitle.Length > 200
        ? Results.ValidationProblem(new Dictionary<string, string[]> { ["title"] = ["Title is required and must be 200 characters or fewer."] })
        : null;
}

static TodoResponse ToResponse(TodoItem todo) => new(todo.Id, todo.Title, todo.IsCompleted, todo.CreatedAtUtc);

public partial class Program;
