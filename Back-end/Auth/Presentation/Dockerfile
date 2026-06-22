# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5014
EXPOSE 5015


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Back-end/Auth/Presentation/Auth.csproj", "Back-end/Auth/Presentation/"]
COPY ["Orchestration/Punto de Venta - Conexión Celular.ServiceDefaults/Punto de Venta - Conexión Celular.ServiceDefaults.csproj", "Orchestration/Punto de Venta - Conexión Celular.ServiceDefaults/"]
COPY ["Shared/Models/Models.csproj", "Shared/Models/"]
COPY ["Back-end/ErrorHandling/ErrorHandling.csproj", "Back-end/ErrorHandling/"]
COPY ["Back-end/Auth/Core/Core.csproj", "Back-end/Auth/Core/"]
COPY ["Shared/DTOs/DTOs.csproj", "Shared/DTOs/"]
COPY ["Back-end/Auth/Infrastructure/Infrastructure.csproj", "Back-end/Auth/Infrastructure/"]
RUN dotnet restore "./Back-end/Auth/Presentation/Auth.csproj"
COPY . .
WORKDIR "/src/Back-end/Auth/Presentation"
RUN dotnet build "./Auth.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Auth.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.dll"]