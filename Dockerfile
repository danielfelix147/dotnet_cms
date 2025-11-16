# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/CMS.API/CMS.API.csproj", "src/CMS.API/"]
COPY ["src/CMS.Application/CMS.Application.csproj", "src/CMS.Application/"]
COPY ["src/CMS.Domain/CMS.Domain.csproj", "src/CMS.Domain/"]
COPY ["src/CMS.Infrastructure/CMS.Infrastructure.csproj", "src/CMS.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/CMS.API/CMS.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/CMS.API"
RUN dotnet build "CMS.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create directory for uploads
RUN mkdir -p /app/wwwroot/uploads

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CMS.API.dll"]
