# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["ClubeRank.Api/ClubeRank.Api.csproj", "ClubeRank.Api/"]
COPY ["ClubeRank.Application/ClubeRank.Application.csproj", "ClubeRank.Application/"]
COPY ["ClubeRank.Domain/ClubeRank.Domain.csproj", "ClubeRank.Domain/"]
COPY ["ClubeRank.Infrastructure/ClubeRank.Infrastructure.csproj", "ClubeRank.Infrastructure/"]
COPY ["ClubeRank.Infrastructure.Data/ClubeRank.Infrastructure.Data.csproj", "ClubeRank.Infrastructure.Data/"]
COPY ["ClubeRank.Infrastructure.Identity/ClubeRank.Infrastructure.Identity.csproj", "ClubeRank.Infrastructure.Identity/"]
COPY ["ClubeRank.Infrastructure.Storage/ClubeRank.Infrastructure.Storage.csproj", "ClubeRank.Infrastructure.Storage/"]
COPY ["ClubeRank.Infrastructure.CrossCutting/ClubeRank.Infrastructure.CrossCutting.csproj", "ClubeRank.Infrastructure.CrossCutting/"]

RUN dotnet restore "ClubeRank.Api/ClubeRank.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ClubeRank.Api"
RUN dotnet build "ClubeRank.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ClubeRank.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClubeRank.Api.dll"]