# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["SGRIA.Api/SGRIA.Api.csproj", "SGRIA.Api/"]
COPY ["SGRIA.Infrastructure/SGRIA.Infrastructure.csproj", "SGRIA.Infrastructure/"]
COPY ["SGRIA.Domain/SGRIA.Domain.csproj", "SGRIA.Domain/"]
COPY ["SGRIA.Application/SGRIA.Application.csproj", "SGRIA.Application/"]

RUN dotnet restore "SGRIA.Api/SGRIA.Api.csproj"

# Copiar todo el código y compilar
COPY . .
WORKDIR "/src/SGRIA.Api"
RUN dotnet build "SGRIA.Api.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "SGRIA.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final: Ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SGRIA.Api.dll"]