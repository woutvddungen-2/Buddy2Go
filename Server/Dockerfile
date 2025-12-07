# Stage 1: Build and publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Build args from script.bat (Release, Debug)
ARG DOTNET_CONFIG=Release

WORKDIR /src
COPY . .

# Build
RUN dotnet publish "Server/Server.csproj" -c $DOTNET_CONFIG -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
LABEL project="Buddy2Go.Server"

# Build arg to control SSL default (0 = off, 1 = on)
ARG USE_SSL=0

# build arg to control Port
ARG PORT=5001

# Default environment variables for the app
ENV SSL__Enabled=${USE_SSL}
ENV PORT=${PORT}

WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /https

EXPOSE ${PORT}
CMD ["sh", "-c", "echo 'Waiting 15 seconds for DB...'; sleep 15; dotnet Server.dll"]
