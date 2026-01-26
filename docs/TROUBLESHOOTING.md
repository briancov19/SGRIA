# 🔧 Troubleshooting - SGRIA

Guía para resolver problemas comunes al trabajar con SGRIA.

## ❌ Error: "Failed to connect to 127.0.0.1:5432"

**Causa:** PostgreSQL no está corriendo o no es accesible.

### Solución 1: Levantar Docker (Recomendado)

```bash
# Verificar si Docker está corriendo
docker compose ps

# Si no hay contenedores, levantarlos
docker compose up -d

# Verificar que PostgreSQL esté corriendo
docker compose ps
```

Deberías ver algo como:
```
NAME          STATUS    PORTS
sgria-db      Up        0.0.0.0:5432->5432/tcp
```

### Solución 2: Verificar Connection String

Cuando ejecutas `dotnet ef` desde fuera de Docker, asegúrate de usar `localhost`:

**appsettings.Development.json** (ya existe):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=appdb;Username=appuser;Password=apppass"
  }
}
```

**appsettings.json** (para Docker):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db;Port=5432;Database=appdb;Username=appuser;Password=apppass"
  }
}
```

### Solución 3: Especificar Environment

```bash
# Asegúrate de estar en modo Development
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet ef database update -p SGRIA.Infrastructure -s SGRIA.Api
```

---

## ❌ Error: "Cannot connect to database" al ejecutar la API

**Causa:** La API está intentando conectarse a `db` (nombre del servicio Docker) pero estás ejecutando fuera de Docker.

### Solución:

1. **Opción A:** Ejecutar dentro de Docker
```bash
docker compose up api
```

2. **Opción B:** Cambiar connection string para desarrollo local
   - Usa `appsettings.Development.json` que ya tiene `localhost`
   - Ejecuta: `dotnet run --project SGRIA.Api`

---

## ❌ Error: "Migration not found"

**Causa:** Las migraciones no se han aplicado a la base de datos.

### Solución:

```bash
# Aplicar todas las migraciones pendientes
dotnet ef database update -p SGRIA.Infrastructure -s SGRIA.Api

# Si estás en Docker, la API aplica migraciones automáticamente al iniciar
```

---

## ❌ Error: "Port already in use"

**Causa:** El puerto 5000 (o el configurado) ya está en uso.

### Solución:

1. **Ver qué proceso usa el puerto:**
```bash
# Windows
netstat -ano | findstr :5000

# Linux/Mac
lsof -i :5000
```

2. **Detener el proceso o cambiar el puerto:**
   - Edita `launchSettings.json`
   - Cambia el puerto en `applicationUrl`

---

## ❌ Error: "Build failed" o errores de compilación

### Solución:

```bash
# Limpiar y reconstruir
dotnet clean
dotnet build

# Restaurar paquetes
dotnet restore
```

---

## ❌ Error: "Docker not found"

**Causa:** Docker no está instalado o no está en el PATH.

### Solución:

1. **Instalar Docker Desktop:**
   - Windows: https://www.docker.com/products/docker-desktop
   - Verificar instalación: `docker --version`

2. **Verificar que Docker esté corriendo:**
   - Abre Docker Desktop
   - Verifica que el ícono esté verde

---

## ❌ Error: "Table already exists" en migraciones

**Causa:** La tabla ya existe en la base de datos pero no está en el historial de migraciones.

### Solución:

```bash
# Marcar migración como aplicada sin ejecutarla
dotnet ef database update <NombreMigracion> -p SGRIA.Infrastructure -s SGRIA.Api

# O eliminar y recrear la base de datos (¡CUIDADO: pierdes datos!)
# Solo en desarrollo
```

---

## ✅ Verificación Rápida

### Checklist de Diagnóstico:

- [ ] Docker está corriendo (`docker compose ps`)
- [ ] PostgreSQL está accesible en `localhost:5432`
- [ ] Variables de entorno están configuradas (`.env` o `docker-compose.yml`)
- [ ] Connection string usa `localhost` para desarrollo local
- [ ] Connection string usa `db` para Docker
- [ ] Migraciones aplicadas (`dotnet ef database update`)
- [ ] API compila sin errores (`dotnet build`)

---

## 🔍 Comandos Útiles de Diagnóstico

```bash
# Ver logs de Docker
docker compose logs db
docker compose logs api

# Ver estado de contenedores
docker compose ps

# Reiniciar servicios
docker compose restart

# Ver variables de entorno de un contenedor
docker compose exec db env

# Conectarse a PostgreSQL desde Docker
docker compose exec db psql -U appuser -d appdb

# Ver migraciones aplicadas
dotnet ef migrations list -p SGRIA.Infrastructure -s SGRIA.Api
```

---

## 📞 Obtener Ayuda

Si el problema persiste:

1. Revisa los logs: `docker compose logs`
2. Verifica la configuración en `appsettings.json`
3. Asegúrate de que todas las dependencias estén instaladas
4. Consulta la documentación en `docs/`

---

**Última actualización:** Enero 2026
