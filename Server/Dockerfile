FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the solution file
COPY ../Buddy2Go.sln ./

# Copy all project files (Server + Shared)
COPY ../Server/Server.csproj ./Server/
COPY ../Shared/Shared.csproj ./Shared/

# Restore dependencies for the solution
RUN dotnet restore

# Copy all source code
COPY ../Server ./Server
COPY ../Shared ./Shared

# Publish the Server project
WORKDIR /app/Server
RUN dotnet publish -c Release -o out

# ----------------------------
# Stage 2: Runtime
# ----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/Server/out ./

# Expose API port
EXPOSE 5000

# Start the API
ENTRYPOINT ["dotnet", "Server.dll"]
