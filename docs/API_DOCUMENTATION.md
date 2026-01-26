# 📡 Documentación de API - SGRIA

Documentación completa de todos los endpoints disponibles en la API REST de SGRIA.

## 🔗 Base URL

```
http://localhost:5000/api
```

## 📋 Índice

1. [Flujo Cliente (Anónimo)](#flujo-cliente-anónimo)
2. [Estadísticas Restaurante](#estadísticas-restaurante)
3. [Gestión de Mesas](#gestión-de-mesas)
4. [Gestión de Productos](#gestión-de-productos)
5. [Notificaciones](#notificaciones)

---

## 🔄 Flujo Cliente (Anónimo)

### 1. Crear o Reutilizar Sesión desde QR

Resuelve una mesa desde su QR token y crea una nueva sesión o reutiliza una sesión activa existente.

**Endpoint:** `POST /api/mesas/qr/{qrToken}/sesion`

**Parámetros:**
- `qrToken` (path, string, requerido) - Token QR único de la mesa

**Body (opcional):**
```json
{
  "cantidadPersonas": 2,
  "origen": "QR"
}
```

**Campos del Body:**
- `cantidadPersonas` (int, opcional) - Número de personas en la mesa
- `origen` (string, opcional) - Origen de la sesión: "QR", "Manual", "Sistema" (default: "QR")

**Respuesta Exitosa (200 OK):**
```json
{
  "id": 1,
  "mesaId": 5,
  "fechaHoraInicio": "2026-01-25T20:00:00Z",
  "fechaHoraFin": null,
  "cantidadPersonas": 2,
  "origen": "QR"
}
```

**Errores:**
- `400 Bad Request` - Mesa no encontrada o no activa
- `404 Not Found` - QR token inválido

**Ejemplo cURL:**
```bash
curl -X POST "http://localhost:5000/api/mesas/qr/MESA-001/sesion" \
  -H "Content-Type: application/json" \
  -d '{
    "cantidadPersonas": 2,
    "origen": "QR"
  }'
```

**Comportamiento:**
- Si existe una sesión activa (sin `fechaHoraFin`) para esa mesa, la reutiliza
- Si no existe, crea una nueva sesión
- Todas las fechas están en UTC

---

### 2. Confirmar Pedido

Confirma que un cliente pidió un item del menú en una sesión específica.

**Endpoint:** `POST /api/sesiones/{sesionId}/pedidos`

**Parámetros:**
- `sesionId` (path, int, requerido) - ID de la sesión de mesa

**Body:**
```json
{
  "itemMenuId": 1,
  "cantidad": 2,
  "ingresadoPor": "Cliente",
  "confianza": 0.95
}
```

**Campos del Body:**
- `itemMenuId` (int, requerido) - ID del item de menú pedido
- `cantidad` (int, opcional) - Cantidad pedida (default: 1)
- `ingresadoPor` (string, opcional) - "Cliente", "Mozo", "Sistema" (default: "Cliente")
- `confianza` (decimal, opcional) - Nivel de confianza 0-1 (para futuro uso)

**Respuesta Exitosa (201 Created):**
```json
{
  "id": 10,
  "sesionMesaId": 1,
  "itemMenuId": 1,
  "itemMenuNombre": "Pizza Margherita",
  "cantidad": 2,
  "fechaHoraConfirmacion": "2026-01-25T20:05:00Z",
  "ingresadoPor": "Cliente",
  "confianza": 0.95
}
```

**Errores:**
- `400 Bad Request` - Sesión cerrada, item no encontrado o inactivo
- `404 Not Found` - Sesión no encontrada

**Ejemplo cURL:**
```bash
curl -X POST "http://localhost:5000/api/sesiones/1/pedidos" \
  -H "Content-Type: application/json" \
  -d '{
    "itemMenuId": 1,
    "cantidad": 2,
    "ingresadoPor": "Cliente"
  }'
```

**Validaciones:**
- La sesión debe estar activa (sin `fechaHoraFin`)
- El item de menú debe existir y estar activo
- La cantidad debe ser mayor a 0

---

### 3. Obtener Pedido

Obtiene la información de un pedido específico.

**Endpoint:** `GET /api/sesiones/pedidos/{pedidoId}`

**Parámetros:**
- `pedidoId` (path, int, requerido) - ID del pedido

**Respuesta Exitosa (200 OK):**
```json
{
  "id": 10,
  "sesionMesaId": 1,
  "itemMenuId": 1,
  "itemMenuNombre": "Pizza Margherita",
  "cantidad": 2,
  "fechaHoraConfirmacion": "2026-01-25T20:05:00Z",
  "ingresadoPor": "Cliente",
  "confianza": 0.95
}
```

**Errores:**
- `404 Not Found` - Pedido no encontrado

---

### 4. Registrar Rating

Registra o actualiza el rating de un pedido. Permite calificar con 👍 (1), 😐 (0), o 👎 (-1).

**Endpoint:** `POST /api/pedidos/{pedidoId}/rating`

**Parámetros:**
- `pedidoId` (path, int, requerido) - ID del pedido a calificar

**Body:**
```json
{
  "puntaje": 1
}
```

**Campos del Body:**
- `puntaje` (short, requerido) - Rating: `-1` (👎), `0` (😐), `1` (👍)

**Respuesta Exitosa (200 OK):**
```json
{
  "id": 5,
  "senalPedidoId": 10,
  "puntaje": 1,
  "fechaHora": "2026-01-25T20:10:00Z"
}
```

**Errores:**
- `400 Bad Request` - Puntaje inválido (debe ser -1, 0 o 1) o pedido no encontrado

**Ejemplo cURL:**
```bash
curl -X POST "http://localhost:5000/api/pedidos/10/rating" \
  -H "Content-Type: application/json" \
  -d '{
    "puntaje": 1
  }'
```

**Comportamiento:**
- Si el pedido ya tiene un rating, lo actualiza
- Si no tiene rating, crea uno nuevo
- Un pedido solo puede tener un rating (relación 1:1)

---

## 📊 Estadísticas Restaurante

### 1. Ranking de Platos Más Pedidos

Obtiene el ranking de platos más pedidos en un período específico.

**Endpoint:** `GET /api/restaurantes/{id}/ranking`

**Parámetros:**
- `id` (path, int, requerido) - ID del restaurante
- `periodo` (query, string, opcional) - Período: `1d`, `7d`, `30d`, `90d` (default: `7d`)

**Valores de Período:**
- `1d` o `1dia` o `today` - Últimas 24 horas
- `7d` o `7dias` o `semana` - Últimos 7 días
- `30d` o `30dias` o `mes` - Últimos 30 días
- `90d` o `90dias` o `trimestre` - Últimos 90 días

**Respuesta Exitosa (200 OK):**
```json
{
  "restauranteId": 1,
  "periodo": "7d",
  "fechaDesde": "2026-01-18T00:00:00Z",
  "fechaHasta": "2026-01-25T23:59:59Z",
  "items": [
    {
      "itemMenuId": 1,
      "nombre": "Pizza Margherita",
      "categoria": "Pizzas",
      "precio": 15.99,
      "totalPedidos": 45,
      "totalCantidad": 67,
      "promedioRating": 0.85,
      "totalRatings": 40
    },
    {
      "itemMenuId": 2,
      "nombre": "Pasta Carbonara",
      "categoria": "Pastas",
      "precio": 12.50,
      "totalPedidos": 32,
      "totalCantidad": 35,
      "promedioRating": 0.92,
      "totalRatings": 28
    }
  ]
}
```

**Campos de Respuesta:**
- `totalPedidos` - Número de veces que se pidió este item
- `totalCantidad` - Cantidad total de unidades pedidas
- `promedioRating` - Promedio de ratings (puede ser null si no hay ratings)
- `totalRatings` - Número de ratings recibidos

**Errores:**
- `400 Bad Request` - Período inválido

**Ejemplo cURL:**
```bash
curl "http://localhost:5000/api/restaurantes/1/ranking?periodo=7d"
```

**Ordenamiento:**
- Primero por `totalPedidos` (descendente)
- Luego por `totalCantidad` (descendente)

---

### 2. Trending - Lo que se está pidiendo ahora

Obtiene los platos que se están pidiendo en tiempo real (últimos X minutos).

**Endpoint:** `GET /api/restaurantes/{id}/trending`

**Parámetros:**
- `id` (path, int, requerido) - ID del restaurante
- `min` (query, int, opcional) - Minutos hacia atrás (default: 30, máximo: 1440)

**Respuesta Exitosa (200 OK):**
```json
{
  "restauranteId": 1,
  "minutos": 30,
  "timestamp": "2026-01-25T20:15:00Z",
  "items": [
    {
      "itemMenuId": 1,
      "nombre": "Pizza Margherita",
      "categoria": "Pizzas",
      "pedidosUltimosMinutos": 8,
      "ultimoPedido": "2026-01-25T20:14:30Z"
    },
    {
      "itemMenuId": 3,
      "nombre": "Ensalada César",
      "categoria": "Ensaladas",
      "pedidosUltimosMinutos": 5,
      "ultimoPedido": "2026-01-25T20:12:15Z"
    }
  ]
}
```

**Errores:**
- `400 Bad Request` - El parámetro `min` debe estar entre 1 y 1440

**Ejemplo cURL:**
```bash
curl "http://localhost:5000/api/restaurantes/1/trending?min=30"
```

**Ordenamiento:**
- Primero por `pedidosUltimosMinutos` (descendente)
- Luego por `ultimoPedido` (descendente)

---

### 3. Platos Más Recomendados

Obtiene el ranking de platos más recomendados basado en el promedio de ratings.

**Endpoint:** `GET /api/restaurantes/{id}/recomendados`

**Parámetros:**
- `id` (path, int, requerido) - ID del restaurante
- `dias` (query, int, opcional) - Días hacia atrás (default: 30, máximo: 365)

**Respuesta Exitosa (200 OK):**
```json
{
  "restauranteId": 1,
  "dias": 30,
  "fechaDesde": "2025-12-26T00:00:00Z",
  "fechaHasta": "2026-01-25T23:59:59Z",
  "minimoRatings": 5,
  "items": [
    {
      "itemMenuId": 2,
      "nombre": "Pasta Carbonara",
      "categoria": "Pastas",
      "precio": 12.50,
      "promedioRating": 0.92,
      "totalRatings": 28,
      "ratingsPositivos": 25,
      "ratingsNeutros": 2,
      "ratingsNegativos": 1
    },
    {
      "itemMenuId": 1,
      "nombre": "Pizza Margherita",
      "categoria": "Pizzas",
      "precio": 15.99,
      "promedioRating": 0.85,
      "totalRatings": 40,
      "ratingsPositivos": 34,
      "ratingsNeutros": 4,
      "ratingsNegativos": 2
    }
  ]
}
```

**Campos de Respuesta:**
- `promedioRating` - Promedio de ratings (-1 a 1)
- `totalRatings` - Total de ratings recibidos
- `ratingsPositivos` - Cantidad de 👍 (puntaje = 1)
- `ratingsNeutros` - Cantidad de 😐 (puntaje = 0)
- `ratingsNegativos` - Cantidad de 👎 (puntaje = -1)

**Filtros:**
- Solo incluye items con mínimo 5 ratings (configurable)

**Errores:**
- `400 Bad Request` - El parámetro `dias` debe estar entre 1 y 365

**Ejemplo cURL:**
```bash
curl "http://localhost:5000/api/restaurantes/1/recomendados?dias=30"
```

**Ordenamiento:**
- Primero por `promedioRating` (descendente)
- Luego por `totalRatings` (descendente)

---

## 🪑 Gestión de Mesas

### 1. Listar Todas las Mesas

**Endpoint:** `GET /api/mesas`

**Respuesta Exitosa (200 OK):**
```json
[
  {
    "id": 1,
    "numero": 1,
    "cantidadSillas": 4,
    "fechaModificacion": "2026-01-25T10:00:00Z"
  }
]
```

### 2. Obtener Mesa por ID

**Endpoint:** `GET /api/mesas/{id}`

**Parámetros:**
- `id` (path, int, requerido) - ID de la mesa

**Respuesta Exitosa (200 OK):**
```json
{
  "id": 1,
  "numero": 1,
  "cantidadSillas": 4,
  "fechaModificacion": "2026-01-25T10:00:00Z"
}
```

### 3. Crear Mesa

**Endpoint:** `POST /api/mesas`

**Body:**
```json
{
  "numero": 5,
  "cantidadSillas": 6
}
```

---

## 🍕 Gestión de Productos

### 1. Listar Todos los Productos

**Endpoint:** `GET /api/productos`

### 2. Obtener Producto por ID

**Endpoint:** `GET /api/productos/{id}`

### 3. Crear Producto

**Endpoint:** `POST /api/productos`

---

## 🔔 Notificaciones

### 1. Listar Notificaciones

**Endpoint:** `GET /api/notificaciones`

### 2. Crear Notificación

**Endpoint:** `POST /api/notificaciones`

---

## 📝 Códigos de Estado HTTP

- `200 OK` - Solicitud exitosa
- `201 Created` - Recurso creado exitosamente
- `400 Bad Request` - Solicitud inválida (validación fallida)
- `404 Not Found` - Recurso no encontrado
- `500 Internal Server Error` - Error del servidor

## 🔒 Seguridad

- ✅ **Sin autenticación** - Todo es anónimo
- ✅ **No se exponen IDs internos** - Se usan QR tokens en lugar de IDs de mesa
- ✅ **Validaciones** - Todas las entradas son validadas
- ✅ **UTC** - Todas las fechas están en UTC

## 📚 Swagger

La documentación interactiva está disponible en:
```
http://localhost:5000/swagger
```

---

**Última actualización:** Enero 2026
