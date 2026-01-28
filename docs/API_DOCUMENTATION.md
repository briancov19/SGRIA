# 📡 Documentación de API - SGRIA

Documentación completa de todos los endpoints disponibles en la API REST de SGRIA.

## 🔗 Base URL

```
http://localhost:5000/api
```

## 📋 Índice

1. [Flujo Cliente (Anónimo)](#flujo-cliente-anónimo)
2. [Estadísticas Públicas desde Sesión](#estadísticas-públicas-desde-sesión)
3. [Estadísticas Restaurante (Admin)](#estadísticas-restaurante-admin)
4. [Gestión de Mesas](#gestión-de-mesas)
5. [Notificaciones](#notificaciones)
6. [Feed Social y Tags](#feed-social-y-tags)

---

## 🔄 Flujo Cliente (Anónimo)

### 1. Crear o Reutilizar Sesión desde QR

Resuelve una mesa desde su QR token y crea una nueva sesión o reutiliza una sesión activa existente. **Devuelve un token público (`sesPublicToken`) que debe usarse en todos los endpoints públicos posteriores.**

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
  "sesPublicToken": "550e8400-e29b-41d4-a716-446655440000",
  "fechaHoraInicio": "2026-01-25T20:00:00Z",
  "fechaHoraFin": null,
  "cantidadPersonas": 2,
  "origen": "QR"
}
```

**⚠️ Importante:** 
- El campo `sesPublicToken` es un GUID único que identifica la sesión públicamente
- **NO se expone** el `id` interno ni el `mesaId` por seguridad
- Este token debe guardarse y usarse en todos los endpoints públicos posteriores

**Headers:**
- `X-Client-Id` (opcional) - GUID del dispositivo para identificación anónima. Si no se envía, el servidor genera uno y lo devuelve en la respuesta.

**Errores:**
- `400 Bad Request` - Mesa no encontrada o no activa
- `404 Not Found` - QR token inválido
- `409 Conflict` - Mesa no activa

**Ejemplo cURL:**
```bash
curl -X POST "http://localhost:5000/api/mesas/qr/MESA-001/sesion" \
  -H "Content-Type: application/json" \
  -H "X-Client-Id: 550e8400-e29b-41d4-a716-446655440000" \
  -d '{
    "cantidadPersonas": 2,
    "origen": "QR"
  }'
```

**Comportamiento:**
- Si existe una sesión activa (sin `fechaHoraFin`) para esa mesa con actividad reciente (últimos 90 minutos), la reutiliza
- Si la sesión existente expiró (más de 90 minutos sin actividad), se cierra automáticamente y se crea una nueva
- Todas las fechas están en UTC
- El timeout de sesión es configurable en `appsettings.json` bajo `Session:TimeoutMinutes` (default: 90)

---

### 2. Confirmar Pedido

Confirma que un cliente pidió un item del menú en una sesión específica usando el token público.

**Endpoint:** `POST /api/sesiones/{sesPublicToken}/pedidos`

**Parámetros:**
- `sesPublicToken` (path, string, requerido) - Token público de la sesión (obtenido al crear/reutilizar sesión)

**Body:**
```json
{
  "itemMenuId": 1,
  "cantidad": 2,
  "ingresadoPor": "Cliente"
}
```

**Campos del Body:**
- `itemMenuId` (int, requerido) - ID del item de menú pedido
- `cantidad` (int, opcional) - Cantidad pedida (default: 1)
- `ingresadoPor` (string, opcional) - "Cliente", "Mozo", "Sistema" (default: "Cliente")

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

**Headers:**
- `X-Client-Id` (**requerido**) - GUID del dispositivo. Obtenerlo al escanear el QR (`POST /api/mesas/qr/{qrToken}/sesion`). Necesario para rate limiting y actividad reciente.

**Errores:**
- `400 Bad Request` - Falta `X-Client-Id`, item no encontrado, inactivo o no pertenece al restaurante de la sesión
- `409 Conflict` - Sesión expirada, cerrada o "debes escanear el QR para unirte a la sesión"
- `429 Too Many Requests` - Límite de pedidos excedido (máximo 10 pedidos cada 10 minutos por participante)

**Ejemplo cURL:**
```bash
curl -X POST "http://localhost:5000/api/sesiones/550e8400-e29b-41d4-a716-446655440000/pedidos" \
  -H "Content-Type: application/json" \
  -H "X-Client-Id: 550e8400-e29b-41d4-a716-446655440000" \
  -d '{
    "itemMenuId": 1,
    "cantidad": 2,
    "ingresadoPor": "Cliente"
  }'
```

**Validaciones:**
- `X-Client-Id` es obligatorio. Debes haber escaneado el QR y unirte a la sesión antes de confirmar pedidos.
- La sesión debe estar activa (sin `fechaHoraFin`) y no expirada (actividad reciente)
- El item de menú debe existir, estar activo y pertenecer al restaurante de la sesión
- La cantidad debe ser mayor a 0
- Se valida actividad reciente del participante (máximo 10 minutos desde última actividad)
- Se aplica rate limiting por participante

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

**Headers:**
- `X-Client-Id` (**requerido**) - GUID del dispositivo. Obtenerlo al escanear el QR. Necesario para rate limiting y actividad reciente.

**Errores:**
- `400 Bad Request` - Falta `X-Client-Id`, puntaje inválido (debe ser -1, 0 o 1) o pedido no encontrado
- `409 Conflict` - Sesión expirada, sin actividad reciente o "debes escanear el QR para unirte antes de calificar"
- `429 Too Many Requests` - Límite de ratings excedido (máximo 10 ratings cada 10 minutos por participante)

**Ejemplo cURL:**
```bash
curl -X POST "http://localhost:5000/api/pedidos/10/rating" \
  -H "Content-Type: application/json" \
  -H "X-Client-Id: 550e8400-e29b-41d4-a716-446655440000" \
  -d '{
    "puntaje": 1
  }'
```

**Comportamiento:**
- Si el pedido ya tiene un rating, lo actualiza (upsert)
- Si no tiene rating, crea uno nuevo
- Un pedido solo puede tener un rating (relación 1:1)
- Valida que la sesión del pedido esté activa y no expirada
- Se valida actividad reciente del participante (máximo 10 minutos)

---

## 📊 Estadísticas Públicas desde Sesión

Estos endpoints permiten obtener estadísticas usando el token público de la sesión. **El restaurante se obtiene automáticamente desde sesión → mesa → restaurante**, sin necesidad de exponer el `restauranteId`.

### 1. Feed Completo

Obtiene el feed completo (trending, ranking, recomendados) desde un token público de sesión.

**Endpoint:** `GET /api/sesiones/{sesPublicToken}/feed`

**Parámetros:**
- `sesPublicToken` (path, string, requerido) - Token público de la sesión
- `min` (query, int, opcional) - Minutos para trending (default: 30, máximo: 1440)
- `periodo` (query, string, opcional) - Período para ranking: `1d`, `7d`, `30d`, `90d` (default: `7d`)
- `dias` (query, int, opcional) - Días para recomendados (default: 30, máximo: 365)

**Respuesta Exitosa (200 OK):**
```json
{
  "timestamp": "2026-01-26T10:00:00Z",
  "sesPublicToken": "550e8400-e29b-41d4-a716-446655440000",
  "trending": [
    {
      "itemMenuId": 1,
      "nombre": "Pizza Margherita",
      "categoria": "Pizzas",
      "pedidosUltimosMinutos": 8,
      "mesasUltimosMinutos": 5,
      "ultimoPedido": "2026-01-26T09:55:00Z"
    }
  ],
  "ranking": [
    {
      "itemMenuId": 1,
      "nombre": "Pizza Margherita",
      "categoria": "Pizzas",
      "precio": 15.99,
      "totalPedidos": 45,
      "totalCantidad": 67,
      "promedioRating": 0.85,
      "totalRatings": 40
    }
  ],
  "recomendados": [
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
    }
  ]
}
```

**Nota:** El feed público filtra automáticamente pedidos con confianza < 0.3 (configurable en `AntiAbuse:MinConfianzaFeedPublico`).

**Errores:**
- `400 Bad Request` - Parámetros inválidos
- `404 Not Found` - Sesión no encontrada con el token proporcionado
- `409 Conflict` - Sesión expirada

**Ejemplo cURL:**
```bash
curl "http://localhost:5000/api/sesiones/550e8400-e29b-41d4-a716-446655440000/feed?min=30&periodo=7d&dias=30"
```

---

### 2. Trending - Lo que se está pidiendo ahora

Obtiene los platos que se están pidiendo en tiempo real (últimos X minutos).

**Endpoint:** `GET /api/sesiones/{sesPublicToken}/trending`

**Parámetros:**
- `sesPublicToken` (path, string, requerido) - Token público de la sesión
- `min` (query, int, opcional) - Minutos hacia atrás (default: 30, máximo: 1440)
- `minConfianza` (query, decimal, opcional) - Confianza mínima para filtrar pedidos (0.0-1.0)

**Respuesta Exitosa (200 OK):** *(No incluye `restauranteId`; el restaurante se infiere de la sesión.)*
```json
{
  "minutos": 30,
  "timestamp": "2026-01-25T20:15:00Z",
  "items": [
    {
      "itemMenuId": 1,
      "nombre": "Pizza Margherita",
      "categoria": "Pizzas",
      "pedidosUltimosMinutos": 8,
      "mesasUltimosMinutos": 5,
      "ultimoPedido": "2026-01-25T20:14:30Z"
    }
  ]
}
```

**Errores:**
- `400 Bad Request` - Parámetros inválidos
- `404 Not Found` - Sesión no encontrada
- `409 Conflict` - Sesión expirada

**Ejemplo cURL:**
```bash
curl "http://localhost:5000/api/sesiones/550e8400-e29b-41d4-a716-446655440000/trending?min=30"
```

---

### 3. Ranking de Platos Más Pedidos

Obtiene el ranking de platos más pedidos en un período específico.

**Endpoint:** `GET /api/sesiones/{sesPublicToken}/ranking`

**Parámetros:**
- `sesPublicToken` (path, string, requerido) - Token público de la sesión
- `periodo` (query, string, opcional) - Período: `1d`, `7d`, `30d`, `90d` (default: `7d`)
- `minConfianza` (query, decimal, opcional) - Confianza mínima para filtrar pedidos (0.0-1.0)

**Valores de Período:**
- `1d` o `1dia` o `today` - Últimas 24 horas
- `7d` o `7dias` o `semana` - Últimos 7 días
- `30d` o `30dias` o `mes` - Últimos 30 días
- `90d` o `90dias` o `trimestre` - Últimos 90 días

**Respuesta Exitosa (200 OK):** *(No incluye `restauranteId`; el restaurante se infiere de la sesión.)*
```json
{
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
    }
  ]
}
```

**Errores:**
- `400 Bad Request` - Período inválido
- `404 Not Found` - Sesión no encontrada
- `409 Conflict` - Sesión expirada

**Ejemplo cURL:**
```bash
curl "http://localhost:5000/api/sesiones/550e8400-e29b-41d4-a716-446655440000/ranking?periodo=7d"
```

---

### 4. Plato Más Recomendados

Obtiene el ranking de platos más recomendados basado en el promedio de ratings.

**Endpoint:** `GET /api/sesiones/{sesPublicToken}/recomendados`

**Parámetros:**
- `sesPublicToken` (path, string, requerido) - Token público de la sesión
- `dias` (query, int, opcional) - Días hacia atrás (default: 30, máximo: 365)
- `minConfianza` (query, decimal, opcional) - Confianza mínima para filtrar pedidos (0.0-1.0)

**Respuesta Exitosa (200 OK):** *(No incluye `restauranteId`; el restaurante se infiere de la sesión.)*
```json
{
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
    }
  ]
}
```

**Errores:**
- `400 Bad Request` - Parámetros inválidos
- `404 Not Found` - Sesión no encontrada
- `409 Conflict` - Sesión expirada

**Ejemplo cURL:**
```bash
curl "http://localhost:5000/api/sesiones/550e8400-e29b-41d4-a716-446655440000/recomendados?dias=30"
```

---

## 📊 Estadísticas Restaurante (Admin)

Estos endpoints están diseñados para uso administrativo y requieren conocer el `restauranteId`. Para uso público desde una sesión, use los endpoints de [Estadísticas Públicas desde Sesión](#estadísticas-públicas-desde-sesión).

### 1. Ranking de Platos Más Pedidos

**Endpoint:** `GET /api/restaurantes/{id}/ranking`

**Parámetros:**
- `id` (path, int, requerido) - ID del restaurante
- `periodo` (query, string, opcional) - Período: `1d`, `7d`, `30d`, `90d` (default: `7d`)
- `minConfianza` (query, decimal, opcional) - Confianza mínima para filtrar pedidos (0.0-1.0)

**Respuesta:** Ver formato en [Ranking desde Sesión](#3-ranking-de-platos-más-pedidos)

---

### 2. Trending - Lo que se está pidiendo ahora

**Endpoint:** `GET /api/restaurantes/{id}/trending`

**Parámetros:**
- `id` (path, int, requerido) - ID del restaurante
- `min` (query, int, opcional) - Minutos hacia atrás (default: 30, máximo: 1440)
- `minConfianza` (query, decimal, opcional) - Confianza mínima para filtrar pedidos (0.0-1.0)

**Respuesta:** Ver formato en [Trending desde Sesión](#2-trending---lo-que-se-está-pidiendo-ahora)

---

### 3. Platos Más Recomendados

**Endpoint:** `GET /api/restaurantes/{id}/recomendados`

**Parámetros:**
- `id` (path, int, requerido) - ID del restaurante
- `dias` (query, int, opcional) - Días hacia atrás (default: 30, máximo: 365)
- `minConfianza` (query, decimal, opcional) - Confianza mínima para filtrar pedidos (0.0-1.0)

**Respuesta:** Ver formato en [Recomendados desde Sesión](#4-plato-más-recomendados)

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
  "cantidadSillas": 6,
  "restauranteId": 1
}
```

### 4. Actualizar Mesa

**Endpoint:** `PUT /api/mesas/{id}`

### 5. Eliminar Mesa

**Endpoint:** `DELETE /api/mesas/{id}`

---

## 🔔 Notificaciones

### 1. Crear Notificación desde QR

Crea una notificación de cliente (pedir cuenta) desde el QR token de la mesa. Crea o reutiliza una sesión automáticamente.

**Endpoint:** `POST /api/notificaciones-cliente`

**Body:**
```json
{
  "qrToken": "MESA-001"
}
```

**Campos del Body:**
- `qrToken` (string, requerido) - Token QR único de la mesa

**Respuesta Exitosa (201 Created):**
```json
{
  "id": 1,
  "fechaCreacion": "2026-01-25T20:00:00Z",
  "atendida": false,
  "mesaId": 5,
  "mesaNumero": 1
}
```

**Errores:**
- `400 Bad Request` - QR token requerido o mesa no activa
- `404 Not Found` - Mesa no encontrada con QR token

**Ejemplo cURL:**
```bash
curl -X POST "http://localhost:5000/api/notificaciones-cliente" \
  -H "Content-Type: application/json" \
  -d '{"qrToken": "MESA-001"}'
```

**Comportamiento:**
- Crea o reutiliza una sesión automáticamente si no existe una activa
- Valida que la mesa existe y está activa

### 2. Obtener Notificación por ID

**Endpoint:** `GET /api/notificaciones-cliente/{id}`

### 3. Listar Notificaciones Activas

**Endpoint:** `GET /api/notificaciones-cliente/activas?minutosCorte=15`

### 4. Marcar Notificación como Atendida

**Endpoint:** `PATCH /api/notificaciones-cliente/{id}/atender`

---

## 📱 Feed Social y Tags

### 1. Estadísticas Sociales de un Item

Obtiene estadísticas sociales detalladas de un item de menú específico.

**Endpoint:** `GET /api/items-menu/{itemMenuId}/social`

**Parámetros:**
- `itemMenuId` (path, int, requerido) - ID del item de menú
- `min` (query, int, opcional) - Minutos para estadísticas recientes (default: 30)
- `dias` (query, int, opcional) - Días para ratings (default: 30)
- `periodo` (query, string, opcional) - Período para total pedidos: `1d`, `7d`, `30d`, `90d` (default: `7d`)

**Respuesta Exitosa (200 OK):**
```json
{
  "itemMenuId": 1,
  "nombre": "Pizza Margherita",
  "categoria": "Pizzas",
  "pedidosUltimosMinutos": 8,
  "mesasUltimosMinutos": 5,
  "totalPedidosPeriodo": 45,
  "promedioRating": 0.85,
  "totalRatings": 40,
  "ratingsPositivos": 34,
  "ratingsNeutros": 4,
  "ratingsNegativos": 2
}
```

**Parámetros Adicionales:**
- `minConfianza` (query, decimal, opcional) - Confianza mínima para filtrar pedidos (0.0-1.0)

**Errores:**
- `400 Bad Request` - Parámetros inválidos
- `404 Not Found` - Item no encontrado

**Ejemplo cURL:**
```bash
curl "http://localhost:5000/api/items-menu/1/social?min=30&dias=30&periodo=7d"
```

---

### 2. Tags Rápidos

#### Obtener Tags Activos

**Endpoint:** `GET /api/restaurantes/{id}/tags`

**Parámetros:**
- `id` (path, int, requerido) - ID del restaurante

**Respuesta Exitosa (200 OK):**
```json
[
  {
    "id": 1,
    "nombre": "Pica",
    "tipo": "Sabor",
    "activo": true
  },
  {
    "id": 2,
    "nombre": "Porción grande",
    "tipo": "Porcion",
    "activo": true
  }
]
```

#### Crear o Actualizar Voto de Tag

Crea o actualiza un voto de tag para un item en una sesión usando token público (upsert). Evita spam con índice único.

**Endpoint:** `POST /api/sesiones/{sesPublicToken}/items/{itemMenuId}/tags`

**Parámetros:**
- `sesPublicToken` (path, string, requerido) - Token público de la sesión
- `itemMenuId` (path, int, requerido) - ID del item de menú

**Body:**
```json
{
  "tagId": 1,
  "valor": 1
}
```

**Campos del Body:**
- `tagId` (int, requerido) - ID del tag
- `valor` (short, requerido) - Valor: `+1` o `-1`

**Respuesta Exitosa (200 OK):**
```json
{
  "id": 5,
  "tagId": 1,
  "tagNombre": "Pica",
  "valor": 1,
  "fechaHora": "2026-01-26T10:00:00Z"
}
```

**Headers:**
- `X-Client-Id` (**requerido**) - GUID del dispositivo. Obtenerlo al escanear el QR. Necesario para rate limiting y actividad reciente.

**Errores:**
- `400 Bad Request` - Falta `X-Client-Id`, item no encontrado, tag no encontrado, valor inválido o item no pertenece al restaurante
- `404 Not Found` - Sesión o item no encontrado
- `409 Conflict` - Sesión expirada o "debes escanear el QR para unirte antes de votar tags"
- `429 Too Many Requests` - Límite de votos de tag excedido (máximo 10 cada 10 minutos por participante)

**Ejemplo cURL:**
```bash
curl -X POST "http://localhost:5000/api/sesiones/550e8400-e29b-41d4-a716-446655440000/items/1/tags" \
  -H "Content-Type: application/json" \
  -H "X-Client-Id: 550e8400-e29b-41d4-a716-446655440000" \
  -d '{"tagId": 1, "valor": 1}'
```

**Validaciones:**
- `X-Client-Id` es obligatorio. Debes haber escaneado el QR y unirte a la sesión antes de votar tags.
- La sesión debe estar activa y no expirada; actividad reciente del participante (máx. 10 min)
- El item debe pertenecer al restaurante de la sesión
- El tag debe estar activo
- El valor debe ser +1 o -1
- Rate limiting por participante (votos de tag)
- Upsert: si ya existe voto para (sesion+item+tag), lo actualiza

---

## 📝 Códigos de Estado HTTP

- `200 OK` - Solicitud exitosa
- `201 Created` - Recurso creado exitosamente
- `400 Bad Request` - Solicitud inválida (validación fallida)
- `404 Not Found` - Recurso no encontrado
- `409 Conflict` - Conflicto de estado (ej: sesión expirada, mesa no activa)
- `429 Too Many Requests` - Límite de rate limiting excedido
- `500 Internal Server Error` - Error del servidor

### Códigos Específicos de Anti-Abuso

#### 429 Too Many Requests
Se devuelve cuando se excede el límite de rate limiting:
```json
{
  "error": "Límite de pedidos excedido. Máximo 10 pedidos cada 10 minutos."
}
```

#### 409 Conflict (Sesión Expirada)
Se devuelve cuando se intenta crear un pedido/rating con una sesión que no tiene actividad reciente o está expirada:
```json
{
  "error": "Sesión expirada. Por favor, re-escanea el QR."
}
```

---

## 🔒 Seguridad y Anti-Abuso

### Tokens Públicos (sesPublicToken)

**⚠️ Importante:** Todos los endpoints públicos ahora usan `sesPublicToken` (GUID) en lugar de `sesionId` (int) para evitar enumeración de sesiones.

- ✅ **Sin autenticación** - Todo es anónimo
- ✅ **No se exponen IDs internos** - Se usan QR tokens y tokens públicos de sesión
- ✅ **Validaciones** - Todas las entradas son validadas
- ✅ **UTC** - Todas las fechas están en UTC
- ✅ **Rate Limiting** - Límites por participante para prevenir spam
- ✅ **Score de Confianza** - Cada pedido tiene un score de confianza (0.0-1.0)
- ✅ **Protección QR** - Validación de actividad reciente para crear pedidos/ratings
- ✅ **Timeout de Sesión** - Las sesiones expiran automáticamente después de 90 minutos sin actividad (configurable)

### Header X-Client-Id

Para endpoints que crean o modifican datos, se recomienda enviar el header `X-Client-Id`:

- **Tipo:** String (GUID recomendado)
- **Obligatorio:** No (pero recomendado para mejor experiencia)
- **Comportamiento:**
  - Si no se envía, el servidor genera uno y lo devuelve en el header de respuesta
  - El frontend debe guardarlo en `localStorage` y reutilizarlo
  - Se usa para identificar anónimamente el dispositivo y aplicar rate limiting

**Ejemplo:**
```http
X-Client-Id: 550e8400-e29b-41d4-a716-446655440000
```

**Ver documentación completa:** [Confianza y Anti-Abuso](./CONFIANZA_ANTI_ABUSO.md)

---

## 📚 Swagger

La documentación interactiva está disponible en:
```
http://localhost:5000/swagger
```

---

**Última actualización:** Enero 2026
