namespace GitHubActionsPractice.Api.Contracts;

public record TodoResponse(int Id, string Title, bool IsCompleted, DateTime CreatedAtUtc);
