# Stage 1: Build and publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "Server/Server.csproj" -c Debug -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
LABEL project="Buddy2Go.Server"
LABEL maintainer="YourName"
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5001
CMD ["sh", "-c", "echo 'Waiting 10 seconds for DB...'; sleep 10; dotnet Server.dll --urls http://+:5001"]
