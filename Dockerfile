# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["API web/API web.csproj", "API web/"]
RUN dotnet restore "API web/API web.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/API web"
RUN dotnet build "API web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "API web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API web.dll"]
