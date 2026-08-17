# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Disables the automatic config-file watcher, which can crash on resource-constrained
# containers (like free hosting tiers) due to hitting the inotify instance limit.
ENV DOTNET_hostBuilder__reloadConfigOnChange=false

EXPOSE 8080
CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet WeatherApp.dll"]