FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Back-end/Inventory/Presentation/Inventory.csproj", "Back-end/Inventory/Presentation/"]
COPY ["Orchestration/Punto de Venta - Conexión Celular.ServiceDefaults/Punto de Venta - Conexión Celular.ServiceDefaults.csproj", "Orchestration/Punto de Venta - Conexión Celular.ServiceDefaults/"]
RUN dotnet restore "Back-end/Inventory/Presentation/Inventory.csproj"
COPY . .
WORKDIR "/src/Back-end/Inventory/Presentation"
RUN dotnet build "./Inventory.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Inventory.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Inventory.dll"]
