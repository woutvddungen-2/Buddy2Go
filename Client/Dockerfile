# Stage 1: Build and publish the Blazor app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

RUN dotnet publish "Client/Client.csproj" -c Release -o /app/publish

# Stage 2: Serve with Nginx
FROM nginx:alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

COPY Client/nginx.conf /etc/nginx/nginx.conf
COPY Client/wwwroot/build.appsettings.json /usr/share/nginx/html/appsettings.json

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
