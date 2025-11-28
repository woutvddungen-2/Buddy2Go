# Stage 1: Build and publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG USE_SSL=0
ARG DOTNET_CONFIG=Release

WORKDIR /src
COPY . .

# Build
RUN dotnet publish "Server/Server.csproj" -c $DOTNET_CONFIG -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
LABEL project="Buddy2Go.Server"
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /https

EXPOSE 5001
CMD ["sh", "-c", "echo 'Waiting 10 seconds for DB...'; sleep 10; dotnet Server.dll"]
