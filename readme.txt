wsl -d Ubuntu

Migrar modelo DB:

1. Crear la migración
Primero, genera una nueva migración. Es recomendable darle un nombre descriptivo, como AddMesasTable. Ejecuta el siguiente comando en tu terminal:
dotnet ef migrations add AddMesasTable -p SGRIA.Infrastructure -s SGRIA.Api

2. Actualizar la base de datos
Una vez creada la migración, debes impactar esos cambios en el servidor de base de datos de SQL:
dotnet ef database update -p SGRIA.Infrastructure -s SGRIA.Api

Si te equivocas: Si creaste la migración pero aún no has ejecutado el database update y quieres corregir algo, puedes borrar la última migración con:
dotnet ef migrations remove -p SGRIA.Infrastructure -s SGRIA.Api

Ver el SQL generado: Si quieres ver qué comandos SQL se van a ejecutar antes de aplicarlos, puedes usar:
dotnet ef migrations script -p SGRIA.Infrastructure -s SGRIA.Api

=====================================================================================================================
build:
docker compose up --build -d

levantar todo:
docker compose up -d

levantar por contenedor:
docker compose up db pgadmin -d

comprobar si levantaron los contenedores:
docker compose ps

Logs:
docker compose logs -f api

Detener:
docker compose stop
=====================================================================================================================

Requisitos del ambiente – SGRIA
Sistema

Windows 10 / 11 o Linux (Ubuntu 20.04+ recomendado)

.NET

.NET SDK 8.x

.NET ASP.NET Core Runtime 8.x (para servidores)

EF Core

dotnet-ef (tool global)

dotnet tool install --global dotnet-ef

Docker

Docker Engine / Docker Desktop

Docker Compose

Imágenes Docker

postgres:16

dpage/pgadmin4

Puertos

5432 (PostgreSQL)

8080 (pgAdmin)

Variables de Base de Datos

POSTGRES_DB=appdb

POSTGRES_USER=appuser

POSTGRES_PASSWORD=apppass

Paquetes NuGet – SGRIA.Infrastructure

Microsoft.EntityFrameworkCore (8.x)

Microsoft.EntityFrameworkCore.Design (8.x)

Npgsql.EntityFrameworkCore.PostgreSQL (8.x)

Paquetes NuGet – SGRIA.Api

Microsoft.EntityFrameworkCore.Design (8.x)

Configuración

Connection String PostgreSQL

Host=localhost;Port=5432;Database=appdb;Username=appuser;Password=apppass

Comandos iniciales
docker compose up -d
dotnet ef database update -p SGRIA.Infrastructure -s SGRIA.Api
dotnet run --project SGRIA.Api

