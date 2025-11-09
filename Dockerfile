# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["CMS.API/CMS.API.csproj", "CMS.API/"]
COPY ["CMS.Application/CMS.Application.csproj", "CMS.Application/"]
COPY ["CMS.Domain/CMS.Domain.csproj", "CMS.Domain/"]
COPY ["CMS.Infrastructure/CMS.Infrastructure.csproj", "CMS.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "CMS.API/CMS.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/CMS.API"
RUN dotnet build "CMS.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CMS.API.dll"]
