# Use ASP.NET Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/MusicForge.Api/MusicForge.Api.csproj", "src/MusicForge.Api/"]
COPY ["src/MusicForge.Application/MusicForge.Application.csproj", "src/MusicForge.Application/"]
COPY ["src/MusicForge.Domain/MusicForge.Domain.csproj", "src/MusicForge.Domain/"]
COPY ["src/MusicForge.Infrastructure/MusicForge.Infrastructure.csproj", "src/MusicForge.Infrastructure/"]
COPY ["protos/worker.proto", "protos/"]

# Restore dependencies
RUN dotnet restore "src/MusicForge.Api/MusicForge.Api.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/src/MusicForge.Api"
RUN dotnet build "MusicForge.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MusicForge.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MusicForge.Api.dll"]
