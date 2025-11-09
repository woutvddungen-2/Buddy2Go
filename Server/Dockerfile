# Stage 1: Build and publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Publish the Server project
#RUN dotnet publish "Server/Server.csproj" -c Release -o /app/publish
RUN dotnet publish "Server/Server.csproj" -c Debug -o /app/publish

# Stage 2: Run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5001
ENTRYPOINT ["dotnet", "Server.dll", "--urls", "https://+:5001"]
