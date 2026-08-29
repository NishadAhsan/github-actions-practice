namespace GitHubActionsPractice.Api.Contracts;

public record CreateTodoRequest(string? Title);

public record UpdateTodoRequest(string? Title, bool IsCompleted);
