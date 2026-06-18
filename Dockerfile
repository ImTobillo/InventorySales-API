# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/InventorySalesApi.Domain/InventorySalesApi.Domain.csproj", "src/InventorySalesApi.Domain/"]
COPY ["src/InventorySalesApi.Application/InventorySalesApi.Application.csproj", "src/InventorySalesApi.Application/"]
COPY ["src/InventorySalesApi.Infrastructure/InventorySalesApi.Infrastructure.csproj", "src/InventorySalesApi.Infrastructure/"]
COPY ["src/InventorySalesApi.Api/InventorySalesApi.Api.csproj", "src/InventorySalesApi.Api/"]

RUN dotnet restore "src/InventorySalesApi.Api/InventorySalesApi.Api.csproj"

# Copy all source files and build
COPY . .
WORKDIR "/src/src/InventorySalesApi.Api"
RUN dotnet build "InventorySalesApi.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "InventorySalesApi.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventorySalesApi.Api.dll"]
