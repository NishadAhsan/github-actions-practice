using System.Net;
using System.Net.Http.Json;
using GitHubActionsPractice.Api.Contracts;

namespace GitHubActionsPractice.Api.Tests;

public class TodoApiTests : IDisposable
{
    private readonly TodoApiFactory _factory = new();
    private readonly HttpClient _client;

    public TodoApiTests() => _client = _factory.CreateClient();

    [Fact]
    public async Task List_returns_migration_seeds_in_id_order()
    {
        var todos = await _client.GetFromJsonAsync<List<TodoResponse>>("/api/todos/");

        Assert.Collection(todos!,
            todo => Assert.Equal((1, "Create a GitHub Actions workflow", false), (todo.Id, todo.Title, todo.IsCompleted)),
            todo => Assert.Equal((2, "Build and test the .NET application", true), (todo.Id, todo.Title, todo.IsCompleted)));
    }

    [Fact]
    public async Task Create_and_get_return_created_todo_and_location()
    {
        var create = await _client.PostAsJsonAsync("/api/todos/", new CreateTodoRequest("  Write integration tests  "));
        var created = await create.Content.ReadFromJsonAsync<TodoResponse>();

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        Assert.Equal($"/api/todos/{created!.Id}", create.Headers.Location!.ToString());
        Assert.Equal("Write integration tests", created.Title);
        Assert.False(created.IsCompleted);
        Assert.Equal(DateTimeKind.Utc, created.CreatedAtUtc.Kind);

        var fetched = await _client.GetFromJsonAsync<TodoResponse>(create.Headers.Location);
        Assert.Equal(created, fetched);
    }

    [Fact]
    public async Task Put_fully_replaces_mutable_fields()
    {
        var response = await _client.PutAsJsonAsync("/api/todos/1", new UpdateTodoRequest("Updated workflow", true));
        var updated = await response.Content.ReadFromJsonAsync<TodoResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal((1, "Updated workflow", true), (updated!.Id, updated.Title, updated.IsCompleted));
        Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), updated.CreatedAtUtc);
    }

    [Fact]
    public async Task Delete_removes_existing_todo()
    {
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync("/api/todos/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/todos/1")).StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Blank_title_returns_validation_problem(string title)
    {
        var response = await _client.PostAsJsonAsync("/api/todos/", new CreateTodoRequest(title));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Overlength_title_returns_validation_problem()
    {
        var response = await _client.PostAsJsonAsync("/api/todos/", new CreateTodoRequest(new string('a', 201)));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_ids_return_not_found()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/todos/999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PutAsJsonAsync("/api/todos/999", new UpdateTodoRequest("Missing", false))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync("/api/todos/999")).StatusCode);
    }

    [Fact]
    public async Task Health_reports_healthy_database()
    {
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/health")).StatusCode);
    }

    [Fact]
    public async Task Development_diagnostics_has_no_cache_headers()
    {
        var response = await _client.GetAsync("/debug/environment");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("no-store", response.Headers.CacheControl!.ToString());
        Assert.Contains("no-cache", response.Headers.GetValues("Pragma"));
    }

    [Fact]
    public async Task Production_does_not_map_diagnostics()
    {
        using var productionFactory = new TodoApiFactory("Production");
        using var productionClient = productionFactory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await productionClient.GetAsync("/debug/environment")).StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
