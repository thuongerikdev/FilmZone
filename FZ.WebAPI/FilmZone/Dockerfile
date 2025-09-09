# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FilmZone/FZ.WebAPI.csproj", "FilmZone/"]
COPY ["../Services/Auth/FZ.Auth.ApplicationService/FZ.Auth.ApplicationService.csproj", "Services/Auth/FZ.Auth.ApplicationService/"]
COPY ["../Services/Shared/FZ.Constant/FZ.Constant.csproj", "Services/Shared/FZ.Constant/"]
COPY ["../Services/Auth/FZ.Auth.Dtos/FZ.Auth.Dtos.csproj", "Services/Auth/FZ.Auth.Dtos/"]
COPY ["../Services/Auth/FZ.Auth.Infrastructure/FZ.Auth.Infrastructure.csproj", "Services/Auth/FZ.Auth.Infrastructure/"]
COPY ["../Services/Auth/FZ.Auth.Domain/FZ.Auth.Domain.csproj", "Services/Auth/FZ.Auth.Domain/"]
RUN dotnet restore "./FilmZone/FZ.WebAPI.csproj"
COPY . .
WORKDIR "/src/FilmZone"
RUN dotnet build "./FZ.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FZ.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FZ.WebAPI.dll"]