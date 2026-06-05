# ── Build Stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ShivakalaCoaching.sln .
COPY global.json .
COPY src/Shivakala.Core/Shivakala.Core.csproj            src/Shivakala.Core/
COPY src/Shivakala.Infrastructure/Shivakala.Infrastructure.csproj  src/Shivakala.Infrastructure/
COPY src/Shivakala.Web/Shivakala.Web.csproj              src/Shivakala.Web/

RUN dotnet restore

COPY . .
WORKDIR /src/src/Shivakala.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime Stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create uploads directory (bind-mount in production)
RUN mkdir -p wwwroot/uploads

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "Shivakala.Web.dll"]
