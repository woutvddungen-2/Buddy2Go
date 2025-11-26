# Stage 1: Build and publish Blazor WASM
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "Client/Client.csproj" -c Release -o /app/publish

# Stage 2: Nginx
FROM nginx:alpine
LABEL project="Buddy2Go.Client"
LABEL maintainer="YourName"
WORKDIR /usr/share/nginx/html

COPY --from=build /app/publish/wwwroot .
COPY Client/nginx.conf /etc/nginx/nginx.conf
COPY Client/wwwroot/build.appsettings.json /usr/share/nginx/html/appsettings.json

EXPOSE 443
CMD ["sh", "-c", "echo 'Waiting 10 seconds for server...'; sleep 10; nginx -g 'daemon off;'"]
