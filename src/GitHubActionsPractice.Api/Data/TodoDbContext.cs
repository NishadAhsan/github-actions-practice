using GitHubActionsPractice.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GitHubActionsPractice.Api.Data;

public class TodoDbContext(DbContextOptions<TodoDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var todos = modelBuilder.Entity<TodoItem>();
        todos.Property(todo => todo.Title).IsRequired().HasMaxLength(200);
        todos.HasData(
            new TodoItem
            {
                Id = 1,
                Title = "Create a GitHub Actions workflow",
                IsCompleted = false,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new TodoItem
            {
                Id = 2,
                Title = "Build and test the .NET application",
                IsCompleted = true,
                CreatedAtUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}
