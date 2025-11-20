# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution file first
COPY ["PegasusBackend.sln", "./"]

# Copy project files for restore (both Backend and Tests needed for solution restore)
COPY ["PegasusBackend/PegasusBackend.csproj", "PegasusBackend/"]
COPY ["PegasusBackend.Tests/PegasusBackend.Tests.csproj", "PegasusBackend.Tests/"]

# Restore all dependencies
RUN dotnet restore "PegasusBackend.sln"

# Copy all source code
COPY . .

# Build only the main project (not tests)
WORKDIR "/src/PegasusBackend"
RUN dotnet build "PegasusBackend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PegasusBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# IMPORTANT FOR RENDER - Listen on PORT environment variable
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

ENTRYPOINT ["dotnet", "PegasusBackend.dll"]
