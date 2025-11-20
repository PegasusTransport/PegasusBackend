# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["PegasusBackend.sln", "./"]
COPY ["PegasusBackend/PegasusBackend.csproj", "PegasusBackend/"]
COPY ["PegasusBackend.Tests/PegasusBackend.Tests.csproj", "PegasusBackend.Tests/"]

# Restore dependencies
RUN dotnet restore "PegasusBackend.sln"

# Copy all source files
COPY . .

# Run tests - build stoppar här om tester failar!
WORKDIR "/src"
RUN dotnet test "PegasusBackend.Tests/PegasusBackend.Tests.csproj" \
    --configuration Release \
    --no-restore \
    --verbosity normal

# Build application
WORKDIR "/src/PegasusBackend"
RUN dotnet build "PegasusBackend.csproj" \
    -c Release \
    -o /app/build \
    --no-restore

# Publish stage
FROM build AS publish
RUN dotnet publish "PegasusBackend.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

# Copy fonts
COPY ["PegasusBackend/Fonts/", "/app/Fonts/"]

ENTRYPOINT ["dotnet", "PegasusBackend.dll"]