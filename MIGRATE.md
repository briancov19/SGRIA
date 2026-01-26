# 🗄️ Guía de Migraciones - SGRIA

## Aplicar Migraciones

### Opción 1: Con Docker (Recomendado)

```bash
# 1. Asegúrate de que Docker esté corriendo
docker compose ps

# 2. Si no está corriendo, levantarlo
docker compose up -d

# 3. Aplicar migraciones (usa localhost desde tu máquina)
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet ef database update -p SGRIA.Infrastructure -s SGRIA.Api
```

### Opción 2: Sin Docker (PostgreSQL local)

Si tienes PostgreSQL instalado localmente:

1. Asegúrate de que PostgreSQL esté corriendo
2. Crea la base de datos:
```sql
CREATE DATABASE appdb;
CREATE USER appuser WITH PASSWORD 'apppass';
GRANT ALL PRIVILEGES ON DATABASE appdb TO appuser;
```

3. Aplica migraciones:
```bash
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet ef database update -p SGRIA.Infrastructure -s SGRIA.Api
```

---

## Crear Nueva Migración

```bash
dotnet ef migrations add NombreMigracion -p SGRIA.Infrastructure -s SGRIA.Api
```

---

## Ver Migraciones Aplicadas

```bash
dotnet ef migrations list -p SGRIA.Infrastructure -s SGRIA.Api
```

---

## Revertir Migración

```bash
# Revertir última migración
dotnet ef database update NombreMigracionAnterior -p SGRIA.Infrastructure -s SGRIA.Api

# Eliminar última migración (si no se aplicó)
dotnet ef migrations remove -p SGRIA.Infrastructure -s SGRIA.Api
```

---

## Ver SQL Generado

```bash
dotnet ef migrations script -p SGRIA.Infrastructure -s SGRIA.Api
```

---

## Troubleshooting

Si tienes problemas de conexión:

1. **Verificar que PostgreSQL esté corriendo:**
   ```bash
   docker compose ps
   ```

2. **Verificar connection string:**
   - Desarrollo local: `Host=localhost;Port=5432;...`
   - Docker: `Host=db;Port=5432;...`

3. **Ver logs de PostgreSQL:**
   ```bash
   docker compose logs db
   ```

---

**Nota:** La API aplica migraciones automáticamente al iniciar si está corriendo en Docker.
