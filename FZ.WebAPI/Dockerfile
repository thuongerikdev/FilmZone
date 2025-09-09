# ...
WORKDIR /src
COPY ["FZ.WebAPI/FilmZone/FZ.WebAPI.csproj", "FZ.WebAPI/FilmZone/"]
COPY ["FZ.WebAPI/Services/Auth/FZ.Auth.ApplicationService/FZ.Auth.ApplicationService.csproj", "FZ.WebAPI/Services/Auth/FZ.Auth.ApplicationService/"]
COPY ["FZ.WebAPI/Services/Shared/FZ.Constant/FZ.Constant.csproj", "FZ.WebAPI/Services/Shared/FZ.Constant/"]
COPY ["FZ.WebAPI/Services/Auth/FZ.Auth.Dtos/FZ.Auth.Dtos.csproj", "FZ.WebAPI/Services/Auth/FZ.Auth.Dtos/"]
COPY ["FZ.WebAPI/Services/Auth/FZ.Auth.Infrastructure/FZ.Auth.Infrastructure.csproj", "FZ.WebAPI/Services/Auth/FZ.Auth.Infrastructure/"]
COPY ["FZ.WebAPI/Services/Auth/FZ.Auth.Domain/FZ.Auth.Domain.csproj", "FZ.WebAPI/Services/Auth/FZ.Auth.Domain/"]

RUN dotnet restore "FZ.WebAPI/FilmZone/FZ.WebAPI.csproj"

COPY FZ.WebAPI/ FZ.WebAPI/
WORKDIR "/src/FZ.WebAPI/FilmZone"
RUN dotnet build "FZ.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/FZ.WebAPI/FilmZone"
RUN dotnet publish "FZ.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FZ.WebAPI.dll"]
