# ⚡ Quick Start - SGRIA

Guía rápida para empezar a usar SGRIA en 5 minutos.

## 🚀 Setup Rápido

### 1. Levantar Base de Datos

```bash
docker compose up -d
```

Esto levanta PostgreSQL y pgAdmin.

### 2. Aplicar Migraciones

```bash
dotnet ef database update -p SGRIA.Infrastructure -s SGRIA.Api
```

### 3. Ejecutar API

```bash
dotnet run --project SGRIA.Api
```

### 4. Abrir Swagger

```
http://localhost:5000/swagger
```

---

## 🎯 Primeros Pasos

### 1. Crear Datos de Prueba

Necesitas crear manualmente (o vía SQL):

- **Restaurante** con ID 1
- **Mesa** con `QrToken = "MESA-001"` y `RestauranteId = 1`
- **ItemMenu** con ID 1 y `RestauranteId = 1`

### 2. Probar Flujo Cliente

```bash
# 1. Crear sesión desde QR
curl -X POST "http://localhost:5000/api/mesas/qr/MESA-001/sesion" \
  -H "Content-Type: application/json" \
  -d '{"cantidadPersonas": 2}'

# 2. Confirmar pedido (usa el sesionId del paso anterior)
curl -X POST "http://localhost:5000/api/sesiones/1/pedidos" \
  -H "Content-Type: application/json" \
  -d '{"itemMenuId": 1, "cantidad": 2}'

# 3. Calificar pedido (usa el pedidoId del paso anterior)
curl -X POST "http://localhost:5000/api/pedidos/1/rating" \
  -H "Content-Type: application/json" \
  -d '{"puntaje": 1}'
```

### 3. Ver Estadísticas

```bash
# Ranking de platos más pedidos
curl "http://localhost:5000/api/restaurantes/1/ranking?periodo=7d"

# Trending (últimos 30 minutos)
curl "http://localhost:5000/api/restaurantes/1/trending?min=30"

# Platos más recomendados
curl "http://localhost:5000/api/restaurantes/1/recomendados?dias=30"
```

---

## 📚 Documentación Completa

- **[API Documentation](./API_DOCUMENTATION.md)** - Todos los endpoints
- **[Architecture Guide](./ARCHITECTURE.md)** - Arquitectura del sistema
- **[Domain Model](./DOMAIN_MODEL.md)** - Modelo de datos
- **[Usage Examples](./USAGE_EXAMPLES.md)** - Ejemplos prácticos

---

## 🔧 Troubleshooting

### Error: "Cannot connect to database"

**Solución:**
```bash
# Verificar que Docker esté corriendo
docker compose ps

# Si no está corriendo, levantarlo
docker compose up -d
```

### Error: "Migration not found"

**Solución:**
```bash
# Aplicar migraciones
dotnet ef database update -p SGRIA.Infrastructure -s SGRIA.Api
```

### Error: "Port already in use"

**Solución:**
- Cambiar puerto en `launchSettings.json`
- O detener el proceso que usa el puerto

---

## ✅ Checklist de Verificación

- [ ] Docker está corriendo
- [ ] PostgreSQL está accesible
- [ ] Migraciones aplicadas
- [ ] API está corriendo
- [ ] Swagger está accesible
- [ ] Datos de prueba creados

---

**¿Listo?** Continúa con [API Documentation](./API_DOCUMENTATION.md) para detalles completos.
