# 🍽️ SGRIA - Sistema de Gestión de Restaurantes con Interacción Anónima

Sistema SaaS para restaurantes que permite generar interacción anónima entre clientes y obtener estadísticas reales de consumo sin agregar trabajo al restaurante.

## 🎯 Objetivo del Sistema

Los clientes escanean un QR de su mesa, pueden confirmar qué plato pidieron (1 toque) y recomendarlo o no (👍 / 😐 / 👎). El sistema genera:

- 📊 **Ranking de platos más pedidos**
- ⭐ **Ranking de platos más recomendados**
- 🔥 **"Qué se está pidiendo ahora"** en tiempo real

Todo es **anónimo, sin login**. El restaurante ve estadísticas y rankings para tomar decisiones de menú, precios y promociones.

## 🏗️ Arquitectura

El proyecto sigue una arquitectura limpia con separación de responsabilidades:

```
SGRIA/
├── SGRIA.Domain/          # Entidades del dominio
├── SGRIA.Application/     # Lógica de negocio, DTOs, Interfaces
├── SGRIA.Infrastructure/  # Persistencia (EF Core), Repositorios
└── SGRIA.Api/            # Controladores REST, Program.cs
```

### Stack Tecnológico

- **.NET 8** - Framework principal
- **ASP.NET Core 8** - API REST
- **Entity Framework Core 8** - ORM
- **PostgreSQL 16** - Base de datos
- **Docker & Docker Compose** - Contenedorización

## 🚀 Inicio Rápido

### Prerrequisitos

- .NET SDK 8.x
- Docker Desktop
- `dotnet-ef` tool global

```bash
dotnet tool install --global dotnet-ef
```

### Configuración

1. **Clonar el repositorio**
```bash
git clone <repo-url>
cd SGRIA
```

2. **Levantar servicios con Docker**
```bash
docker compose up -d
```

Esto levanta:
- PostgreSQL en puerto `5432`
- pgAdmin en puerto `8080`

3. **Aplicar migraciones**
```bash
dotnet ef database update -p SGRIA.Infrastructure -s SGRIA.Api
```

4. **Ejecutar la API**
```bash
dotnet run --project SGRIA.Api
```

La API estará disponible en `http://localhost:5000` (o el puerto configurado).

5. **Acceder a Swagger**
```
http://localhost:5000/swagger
```

## 📚 Documentación

Documentación completa del proyecto:

- **[📚 Índice de Documentación](./docs/INDEX.md)** - Índice completo de toda la documentación
- **[⚡ Quick Start](./docs/QUICK_START.md)** - Guía rápida para empezar en 5 minutos
- **[📡 API Documentation](./docs/API_DOCUMENTATION.md)** - Documentación completa de todos los endpoints con ejemplos
- **[🏗️ Architecture Guide](./docs/ARCHITECTURE.md)** - Detalles de la arquitectura, capas y diseño
- **[🗃️ Domain Model](./docs/DOMAIN_MODEL.md)** - Modelo de datos, entidades y relaciones
- **[📖 Usage Examples](./docs/USAGE_EXAMPLES.md)** - Ejemplos prácticos con cURL, JavaScript y Python

## 🔌 Endpoints Principales

### Flujo Cliente (Anónimo)

1. **POST** `/api/mesas/qr/{qrToken}/sesion` - Escanear QR y crear/reutilizar sesión
2. **POST** `/api/sesiones/{sesionId}/pedidos` - Confirmar pedido
3. **POST** `/api/pedidos/{pedidoId}/rating` - Calificar pedido (👍/😐/👎)

### Estadísticas Restaurante

1. **GET** `/api/restaurantes/{id}/ranking?periodo=7d` - Ranking de platos más pedidos
2. **GET** `/api/restaurantes/{id}/trending?min=30` - Lo que se está pidiendo ahora
3. **GET** `/api/restaurantes/{id}/recomendados?dias=30` - Platos más recomendados

Ver [API Documentation](./docs/API_DOCUMENTATION.md) para detalles completos.

## 🗃️ Modelo de Datos

### Entidades Principales

- **Restaurante** - Información del restaurante
- **Mesa** - Mesas con QR token único
- **SesionMesa** - Sesión de visita (puede tener múltiples pedidos)
- **ItemMenu** - Platos/bebidas del menú
- **SenalPedido** - Confirmación de pedido
- **SenalRating** - Rating del pedido (-1, 0, 1)

Ver [Domain Model](./docs/DOMAIN_MODEL.md) para detalles completos.

## 🧪 Testing

### Ejemplo de Flujo Completo

```bash
# 1. Crear sesión desde QR (guarda el sesPublicToken de la respuesta)
curl -X POST "http://localhost:5000/api/mesas/qr/MESA-001/sesion" \
  -H "Content-Type: application/json" \
  -H "X-Client-Id: 550e8400-e29b-41d4-a716-446655440000" \
  -d '{"cantidadPersonas": 2, "origen": "QR"}'
# Respuesta: { "sesPublicToken": "…", "fechaHoraInicio": "…", … }

# 2. Confirmar pedido (usa el sesPublicToken del paso 1)
curl -X POST "http://localhost:5000/api/sesiones/<sesPublicToken>/pedidos" \
  -H "Content-Type: application/json" \
  -H "X-Client-Id: 550e8400-e29b-41d4-a716-446655440000" \
  -d '{"itemMenuId": 1, "cantidad": 2}'
# Respuesta 201: { "id": 10, "itemMenuNombre": "…", … }

# 3. Calificar pedido (usa el id del pedido del paso 2)
curl -X POST "http://localhost:5000/api/pedidos/10/rating" \
  -H "Content-Type: application/json" \
  -H "X-Client-Id: 550e8400-e29b-41d4-a716-446655440000" \
  -d '{"puntaje": 1}'

# 4. Ver feed (trending + ranking + recomendados) desde sesión
curl "http://localhost:5000/api/sesiones/<sesPublicToken>/feed?min=30&periodo=7d&dias=30"

# 5. Ver ranking por restaurante (admin)
curl "http://localhost:5000/api/restaurantes/1/ranking?periodo=7d"
```

## 🐳 Docker

### Comandos Útiles

```bash
# Levantar todo
docker compose up -d

# Ver logs
docker compose logs -f api

# Detener
docker compose stop

# Reconstruir
docker compose up --build -d
```

## 📝 Migraciones

```bash
# Crear migración
dotnet ef migrations add NombreMigracion -p SGRIA.Infrastructure -s SGRIA.Api

# Aplicar migración
dotnet ef database update -p SGRIA.Infrastructure -s SGRIA.Api

# Ver SQL generado
dotnet ef migrations script -p SGRIA.Infrastructure -s SGRIA.Api

# Eliminar última migración (si no se aplicó)
dotnet ef migrations remove -p SGRIA.Infrastructure -s SGRIA.Api
```

## 🔒 Seguridad y Privacidad

- ✅ **Sin autenticación** - Todo es anónimo
- ✅ **No se exponen IDs internos** - Se usan QR tokens
- ✅ **UTC en todas las fechas** - Consistencia temporal
- ✅ **Validaciones de negocio** - En capa de servicios

## 📊 Características

- 🚀 **Escalable** - Arquitectura limpia y modular
- ⚡ **Optimizado** - Consultas SQL eficientes con índices
- 📱 **RESTful** - API REST estándar
- 🔍 **Swagger** - Documentación interactiva
- 🐳 **Dockerizado** - Fácil despliegue

## 🤝 Contribuir

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto es privado y propietario.

## 👥 Contacto

Para consultas o soporte, contactar al equipo de desarrollo.

---

**SGRIA** - Sistema de Gestión de Restaurantes con Interacción Anónima
