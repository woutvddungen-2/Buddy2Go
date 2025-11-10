# Stage 1: Build and publish the Blazor app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
#RUN dotnet publish "Client/Client.csproj" -c Release -o /app/publish
RUN dotnet publish "Client/Client.csproj" -c Debug -o /app/publish

# Stage 2: Serve with Nginx
FROM nginx:alpine
COPY --from=build /app/publish /usr/share/nginx/html

COPY Client/nginx-client.conf /etc/nginx/conf.d/default.conf

EXPOSE 5002
CMD ["nginx", "-g", "daemon off;"]
