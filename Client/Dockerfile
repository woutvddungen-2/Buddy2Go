# Stage 1: Build and publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG USE_SSL=0
ARG DOTNET_CONFIG=Release

WORKDIR /src
COPY . .

RUN echo "BUILD ARG - USE_SSL = $USE_SSL"
# Build
RUN dotnet publish "Client/Client.csproj" -c $DOTNET_CONFIG -o /app/publish


# Stage 2 Runtime
FROM nginx:alpine
LABEL project="Buddy2Go.Client"
ARG USE_SSL=0
WORKDIR /usr/share/nginx/html

COPY --from=build /app/publish/wwwroot .

COPY Client/wwwroot/appsettings.json /usr/share/nginx/html/appsettings.json

COPY Client/Build/nginx/nginx.http.conf  /etc/nginx/templates/nginx.http.conf
COPY Client/Build/nginx/nginx.https.conf /etc/nginx/templates/nginx.https.conf

RUN mkdir -p /etc/nginx/ssl

RUN if [ "$USE_SSL" = "1" ]; then \
        cp /etc/nginx/templates/nginx.https.conf /etc/nginx/nginx.conf; \
    else \
        cp /etc/nginx/templates/nginx.http.conf  /etc/nginx/nginx.conf; \
    fi

EXPOSE 80
EXPOSE 443

CMD ["nginx", "-g", "daemon off;"]
