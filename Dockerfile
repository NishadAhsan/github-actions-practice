FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY GitHubActionsPractice.sln ./
COPY src/GitHubActionsPractice.Api/GitHubActionsPractice.Api.csproj src/GitHubActionsPractice.Api/
COPY tests/GitHubActionsPractice.Api.Tests/GitHubActionsPractice.Api.Tests.csproj tests/GitHubActionsPractice.Api.Tests/
RUN dotnet restore GitHubActionsPractice.sln

COPY src/GitHubActionsPractice.Api/ src/GitHubActionsPractice.Api/
RUN dotnet publish src/GitHubActionsPractice.Api/GitHubActionsPractice.Api.csproj --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/todos.db"
EXPOSE 8080
VOLUME ["/app/data"]

COPY --from=build /app/publish ./
USER root
RUN mkdir -p /app/data && chown -R app:app /app/data
USER app
ENTRYPOINT ["dotnet", "GitHubActionsPractice.Api.dll"]
