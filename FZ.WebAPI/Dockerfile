# --- Base runtime (ASP.NET) ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# --- Build stage (SDK) ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# copy csproj để restore
COPY ["FilmZone/FZ.WebAPI.csproj", "FilmZone/"]
COPY ["Services/Auth/FZ.Auth.ApplicationService/FZ.Auth.ApplicationService.csproj", "Services/Auth/FZ.Auth.ApplicationService/"]
COPY ["Services/Shared/FZ.Constant/FZ.Constant.csproj", "Services/Shared/FZ.Constant/"]
COPY ["Services/Auth/FZ.Auth.Dtos/FZ.Auth.Dtos.csproj", "Services/Auth/FZ.Auth.Dtos/"]
COPY ["Services/Auth/FZ.Auth.Infrastructure/FZ.Auth.Infrastructure.csproj", "Services/Auth/FZ.Auth.Infrastructure/"]
COPY ["Services/Auth/FZ.Auth.Domain/FZ.Auth.Domain.csproj", "Services/Auth/FZ.Auth.Domain/"]

RUN dotnet restore "FilmZone/FZ.WebAPI.csproj"

# copy toàn bộ source rồi build
COPY . .
WORKDIR "/src/FilmZone"
RUN dotnet build "FZ.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# --- Publish ---
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FZ.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# --- Final image ---
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Tạo entrypoint: nếu có PFX thì bật HTTPS:8081, không thì chỉ HTTP:8080
RUN mkdir -p /https \
 && printf '#!/bin/sh\nset -e\n\
if [ -n "$KESTREL_PFX_BASE64" ] && [ -n "$KESTREL_PFX_PASSWORD" ]; then \n\
  echo "$KESTREL_PFX_BASE64" | base64 -d > /https/server.pfx; \n\
  export ASPNETCORE_Kestrel__Certificates__Default__Path=/https/server.pfx; \n\
  export ASPNETCORE_Kestrel__Certificates__Default__Password="$KESTREL_PFX_PASSWORD"; \n\
  export ASPNETCORE_URLS="http://+:8080;https://+:8081"; \n\
else \n\
  export ASPNETCORE_URLS="http://+:8080"; \n\
fi\n\
exec dotnet FZ.WebAPI.dll\n' > /entrypoint.sh \
 && chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]
