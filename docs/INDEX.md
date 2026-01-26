# 📚 Índice de Documentación - SGRIA

Índice completo de toda la documentación del proyecto SGRIA.

## 🚀 Empezar Aquí

1. **[Quick Start](./QUICK_START.md)** ⚡
   - Setup rápido en 5 minutos
   - Primeros pasos
   - Troubleshooting básico

2. **[README Principal](../README.md)** 📖
   - Visión general del proyecto
   - Objetivos y características
   - Setup completo

---

## 📡 Documentación de API

### **[API Documentation](./API_DOCUMENTATION.md)** 📡

Documentación completa de todos los endpoints:

#### Flujo Cliente (Anónimo)
- `POST /api/mesas/qr/{qrToken}/sesion` - Crear/reutilizar sesión desde QR
- `POST /api/sesiones/{sesionId}/pedidos` - Confirmar pedido
- `POST /api/pedidos/{pedidoId}/rating` - Calificar pedido (👍/😐/👎)

#### Estadísticas Restaurante
- `GET /api/restaurantes/{id}/ranking` - Ranking de platos más pedidos
- `GET /api/restaurantes/{id}/trending` - Lo que se está pidiendo ahora
- `GET /api/restaurantes/{id}/recomendados` - Platos más recomendados

#### Gestión
- `GET /api/mesas` - Listar mesas
- `POST /api/notificaciones-cliente` - Crear notificación desde QR

#### Feed Social y Tags
- `GET /api/mesas/qr/{qrToken}/feed` - Feed completo (trending, ranking, recomendados)
- `GET /api/items-menu/{id}/social` - Estadísticas sociales de un item
- `GET /api/restaurantes/{id}/tags` - Listar tags activos
- `POST /api/sesiones/{sesionId}/items/{itemMenuId}/tags` - Votar tag (upsert)

**Incluye:**
- ✅ Descripción de cada endpoint
- ✅ Parámetros y body requests
- ✅ Ejemplos de respuestas
- ✅ Códigos de error
- ✅ Ejemplos con cURL

---

## 🏗️ Arquitectura y Diseño

### **[Architecture Guide](./ARCHITECTURE.md)** 🏗️

Documentación detallada de la arquitectura:

- **Estructura de Capas**
  - SGRIA.Domain (Entidades)
  - SGRIA.Application (Lógica de negocio)
  - SGRIA.Infrastructure (Persistencia)
  - SGRIA.Api (Presentación)

- **Patrones de Diseño**
  - Repository Pattern
  - Dependency Injection
  - Clean Architecture

- **Flujo de Datos**
  - Request → Controller → Service → Repository → Database

- **Optimizaciones**
  - Índices de base de datos
  - Consultas eficientes
  - Eager Loading

- **Escalabilidad**
  - Consideraciones futuras
  - Caching
  - Message Queues

---

## 🗃️ Modelo de Datos

### **[Domain Model](./DOMAIN_MODEL.md)** 🗃️

Documentación completa del modelo de datos:

- **Entidades Principales**
  - Restaurante
  - Mesa
  - SesionMesa
  - ItemMenu
  - SenalPedido
  - SenalRating
  - TagRapido
  - VotoTagItemMenu

- **Relaciones**
  - Diagrama de entidades
  - Cardinalidades
  - Foreign Keys

- **Índices**
  - Índices críticos para performance
  - Consultas optimizadas

- **Consultas SQL**
  - Ejemplos de queries comunes
  - Rankings
  - Trending
  - Recomendados

---

## 📖 Ejemplos y Guías

### **[Usage Examples](./USAGE_EXAMPLES.md)** 📖

Ejemplos prácticos de uso:

- **Flujo Completo**
  - Cliente escanea QR
  - Confirma pedido
  - Califica pedido

- **Ejemplos de Código**
  - cURL (bash)
  - JavaScript (Fetch API)
  - Python (Requests)

- **Estadísticas**
  - Obtener rankings
  - Ver trending
  - Consultar recomendados

- **Manejo de Errores**
  - Códigos de error comunes
  - Mensajes de error
  - Soluciones

- **Mejores Prácticas**
  - Manejo de IDs
  - UTC en fechas
  - Reutilización de sesiones

---

## 🗂️ Estructura de Archivos

```
docs/
├── INDEX.md                    # Este archivo
├── QUICK_START.md              # Guía rápida
├── API_DOCUMENTATION.md         # Documentación de endpoints
├── ARCHITECTURE.md              # Arquitectura del sistema
├── DOMAIN_MODEL.md              # Modelo de datos
└── USAGE_EXAMPLES.md           # Ejemplos prácticos
```

---

## 🎯 Guías por Rol

### Para Desarrolladores Backend
1. [Architecture Guide](./ARCHITECTURE.md) - Entender la estructura
2. [Domain Model](./DOMAIN_MODEL.md) - Conocer las entidades
3. [API Documentation](./API_DOCUMENTATION.md) - Implementar endpoints

### Para Desarrolladores Frontend
1. [Quick Start](./QUICK_START.md) - Setup rápido
2. [API Documentation](./API_DOCUMENTATION.md) - Endpoints disponibles
3. [Usage Examples](./USAGE_EXAMPLES.md) - Ejemplos de integración

### Para Product Managers / Stakeholders
1. [README Principal](../README.md) - Visión general
2. [API Documentation](./API_DOCUMENTATION.md) - Funcionalidades
3. [Usage Examples](./USAGE_EXAMPLES.md) - Casos de uso

---

## 🔍 Búsqueda Rápida

### ¿Cómo...?

- **...crear una sesión?** → [API Documentation - Crear Sesión](./API_DOCUMENTATION.md#1-crear-o-reutilizar-sesión-desde-qr)
- **...confirmar un pedido?** → [API Documentation - Confirmar Pedido](./API_DOCUMENTATION.md#2-confirmar-pedido)
- **...obtener rankings?** → [API Documentation - Ranking](./API_DOCUMENTATION.md#1-ranking-de-platos-más-pedidos)
- **...entender la arquitectura?** → [Architecture Guide](./ARCHITECTURE.md)
- **...ver el modelo de datos?** → [Domain Model](./DOMAIN_MODEL.md)
- **...empezar rápido?** → [Quick Start](./QUICK_START.md)

---

## 📝 Convenciones

- **Fechas:** Todas en UTC
- **IDs:** No se exponen en URLs públicas (se usan QR tokens)
- **Ratings:** -1 (👎), 0 (😐), 1 (👍)
- **Códigos HTTP:** 200 (OK), 201 (Created), 400 (Bad Request), 404 (Not Found)

---

## 🔄 Actualizaciones

- **Última actualización:** Enero 2026
- **Versión de API:** 1.0
- **.NET Version:** 8.0
- **EF Core Version:** 8.0

---

**¿Necesitas ayuda?** Revisa la documentación específica o consulta los ejemplos en [Usage Examples](./USAGE_EXAMPLES.md).
