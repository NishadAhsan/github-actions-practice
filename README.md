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

### Private GitHub Container Registry

Stable annotated Git tags publish the API image to GitHub Container Registry. Create and push a release tag from the commit to release:

```sh
git tag -a v1.0.0 -m "v1.0.0"
git push origin v1.0.0
```

The only published tag is the exact stable release version: `ghcr.io/nishadahsan/github-actions-practice:v1.0.0`. Tags must use `vMAJOR.MINOR.PATCH`; the workflow does not publish `latest`, SHA, branch, prerelease, pull request, `main`, or manually dispatched builds.

After the first publish, the package owner must complete this one-time GitHub configuration:

- Confirm the package visibility remains **Private**.
- Disable inherited access from this public repository.
- If GitHub no longer retains it after disabling inheritance, explicitly grant this repository's GitHub Actions access **Write** permission.
- Invite each QA GitHub account with **Read** access.

QA users create a classic personal access token scoped only to `read:packages`, then authenticate and run the image without checking out the source:

```sh
export CR_PAT='your-read-packages-token'
printf '%s' "$CR_PAT" | docker login ghcr.io -u YOUR_GITHUB_USERNAME --password-stdin
docker pull ghcr.io/nishadahsan/github-actions-practice:v1.0.0
docker run --rm -p 8080:8080 -v github-actions-practice-data:/app/data ghcr.io/nishadahsan/github-actions-practice:v1.0.0
```

Verify the API at `http://localhost:8080/health`. The named `github-actions-practice-data` volume preserves SQLite data when containers are replaced. Log out when finished with `docker logout ghcr.io`. Private pulls require authentication even though the source repository is public.

Do not make this package public for this trial: public GHCR package visibility cannot be changed back to private.
