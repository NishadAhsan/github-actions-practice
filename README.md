# Readme

## Github Actions

### Unit 1

#### Basic Workflow and Events

## Todo API

This repository also contains a .NET 10 minimal API backed by SQLite. It is independent from the GitHub Actions practice workflows.

### Prerequisites

- .NET SDK 10.0.400 or later stable .NET 10 feature band
- Docker (optional, for the container image)

Restore the local EF Core tool and build the solution from the repository root:

```sh
dotnet tool restore
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

Run the API in Development mode:

```sh
dotnet run --project src/GitHubActionsPractice.Api --urls http://localhost:8080
```

The API listens on `http://localhost:8080`. A local database is created at `src/GitHubActionsPractice.Api/data/todos.db` by default. Set `ConnectionStrings__DefaultConnection` to override it.

### Endpoints

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/todos` | List todos ordered by ID. |
| `GET` | `/api/todos/{id}` | Get one todo. |
| `POST` | `/api/todos` | Create a todo with `{ "title": "..." }`. |
| `PUT` | `/api/todos/{id}` | Replace mutable fields with `{ "title": "...", "isCompleted": true }`. |
| `DELETE` | `/api/todos/{id}` | Delete a todo. |
| `GET` | `/health` | Check SQLite connectivity. |

Titles are trimmed and must contain 1 through 200 characters. A new database receives two fixed GitHub Actions-themed sample todos through its initial migration.

```sh
curl http://localhost:8080/api/todos
curl -X POST http://localhost:8080/api/todos -H "Content-Type: application/json" -d '{"title":"Add a CI check"}'
curl -X PUT http://localhost:8080/api/todos/1 -H "Content-Type: application/json" -d '{"title":"Create a GitHub Actions workflow","isCompleted":true}'
curl http://localhost:8080/health
```

### Diagnostics Warning

`GET /debug/environment` exists only in the Development environment and returns every process environment variable, including unredacted credentials and tokens. Never expose this route publicly or copy its output into logs, GitHub Actions summaries, or issue comments. In Production, the route is not mapped and returns `404`.

### Docker

Build and run the root application image with a named volume for SQLite persistence:

```sh
docker build -t github-actions-practice .
docker run --rm -p 8080:8080 -v github-actions-practice-data:/app/data github-actions-practice
curl http://localhost:8080/health
```

The container listens on port `8080` and runs as a non-root user. Reusing `github-actions-practice-data` preserves todos across container replacements. Removing that volume removes the SQLite data.
