# 📖 Ejemplos de Uso - SGRIA

Guía práctica con ejemplos de uso de la API SGRIA.

## 🎯 Flujo Completo: Cliente Escanea QR y Pide

### Escenario
Un cliente llega a un restaurante, escanea el QR de su mesa, confirma que pidió una pizza y la califica.

---

## Paso 1: Escanear QR y Crear Sesión

**Request:**
```bash
POST http://localhost:5000/api/mesas/qr/MESA-001/sesion
Content-Type: application/json

{
  "cantidadPersonas": 2,
  "origen": "QR"
}
```

**Response (200 OK):**
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

**Notas:**
- Si ya existe una sesión activa para esa mesa, la reutiliza
- El `id` de la sesión se usará en los siguientes pasos

---

## Paso 2: Confirmar Pedido

**Request:**
```bash
POST http://localhost:5000/api/sesiones/1/pedidos
Content-Type: application/json

{
  "itemMenuId": 1,
  "cantidad": 2,
  "ingresadoPor": "Cliente"
}
```

**Response (201 Created):**
```json
{
  "id": 10,
  "sesionMesaId": 1,
  "itemMenuId": 1,
  "itemMenuNombre": "Pizza Margherita",
  "cantidad": 2,
  "fechaHoraConfirmacion": "2026-01-25T20:05:00Z",
  "ingresadoPor": "Cliente",
  "confianza": null
}
```

**Notas:**
- El `id` del pedido se usará para calificar
- Puedes confirmar múltiples pedidos en la misma sesión

---

## Paso 3: Calificar Pedido

**Request:**
```bash
POST http://localhost:5000/api/pedidos/10/rating
Content-Type: application/json

{
  "puntaje": 1
}
```

**Response (200 OK):**
```json
{
  "id": 5,
  "senalPedidoId": 10,
  "puntaje": 1,
  "fechaHora": "2026-01-25T20:10:00Z"
}
```

**Valores de Puntaje:**
- `1` = 👍 (Me gustó)
- `0` = 😐 (Neutral)
- `-1` = 👎 (No me gustó)

**Notas:**
- Si el pedido ya tiene un rating, se actualiza
- Puedes cambiar el rating en cualquier momento

---

## 📊 Ejemplos de Estadísticas

### 1. Ver Ranking de Platos Más Pedidos (Últimos 7 días)

**Request:**
```bash
GET http://localhost:5000/api/restaurantes/1/ranking?periodo=7d
```

**Response (200 OK):**
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

**Períodos Disponibles:**
- `1d` - Últimas 24 horas
- `7d` - Últimos 7 días (default)
- `30d` - Últimos 30 días
- `90d` - Últimos 90 días

---

### 2. Ver Trending (Lo que se está pidiendo ahora)

**Request:**
```bash
GET http://localhost:5000/api/restaurantes/1/trending?min=30
```

**Response (200 OK):**
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

**Parámetros:**
- `min` - Minutos hacia atrás (default: 30, máximo: 1440 = 24 horas)

**Uso:**
- Perfecto para mostrar en tiempo real qué se está pidiendo
- Útil para promociones dinámicas

---

### 3. Ver Platos Más Recomendados

**Request:**
```bash
GET http://localhost:5000/api/restaurantes/1/recomendados?dias=30
```

**Response (200 OK):**
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

**Parámetros:**
- `dias` - Días hacia atrás (default: 30, máximo: 365)

**Filtros:**
- Solo incluye items con mínimo 5 ratings
- Ordenado por promedio de rating (descendente)

---

## 🔄 Flujos Alternativos

### Flujo: Cliente vuelve a la misma mesa

Si un cliente vuelve a escanear el mismo QR:

**Request:**
```bash
POST http://localhost:5000/api/mesas/qr/MESA-001/sesion
```

**Comportamiento:**
- Si hay una sesión activa (sin `fechaHoraFin`), la reutiliza
- Si no hay sesión activa, crea una nueva

**Response (reutilizando sesión existente):**
```json
{
  "id": 1,  // Mismo ID de sesión anterior
  "mesaId": 5,
  "fechaHoraInicio": "2026-01-25T20:00:00Z",  // Fecha original
  "fechaHoraFin": null,
  "cantidadPersonas": 2,
  "origen": "QR"
}
```

---

### Flujo: Actualizar Rating

Si un cliente quiere cambiar su rating:

**Request:**
```bash
POST http://localhost:5000/api/pedidos/10/rating
Content-Type: application/json

{
  "puntaje": -1  // Cambió de 👍 a 👎
}
```

**Comportamiento:**
- Si ya existe un rating, lo actualiza
- Si no existe, crea uno nuevo

**Response:**
```json
{
  "id": 5,  // Mismo ID (actualizado)
  "senalPedidoId": 10,
  "puntaje": -1,  // Nuevo valor
  "fechaHora": "2026-01-25T20:20:00Z"  // Nueva fecha
}
```

---

## 🧪 Ejemplos con cURL

### Script Completo: Flujo Cliente

```bash
#!/bin/bash

BASE_URL="http://localhost:5000/api"
QR_TOKEN="MESA-001"

# 1. Crear sesión
echo "1. Creando sesión..."
SESION=$(curl -s -X POST "$BASE_URL/mesas/qr/$QR_TOKEN/sesion" \
  -H "Content-Type: application/json" \
  -d '{"cantidadPersonas": 2, "origen": "QR"}')

SESION_ID=$(echo $SESION | jq -r '.id')
echo "Sesión creada: $SESION_ID"

# 2. Confirmar pedido
echo "2. Confirmando pedido..."
PEDIDO=$(curl -s -X POST "$BASE_URL/sesiones/$SESION_ID/pedidos" \
  -H "Content-Type: application/json" \
  -d '{"itemMenuId": 1, "cantidad": 2}')

PEDIDO_ID=$(echo $PEDIDO | jq -r '.id')
echo "Pedido confirmado: $PEDIDO_ID"

# 3. Calificar pedido
echo "3. Calificando pedido..."
curl -s -X POST "$BASE_URL/pedidos/$PEDIDO_ID/rating" \
  -H "Content-Type: application/json" \
  -d '{"puntaje": 1}' | jq

echo "✅ Flujo completado"
```

---

## 📱 Ejemplos con JavaScript (Fetch)

### Crear Sesión y Confirmar Pedido

```javascript
const BASE_URL = 'http://localhost:5000/api';

async function flujoCompleto() {
  try {
    // 1. Crear sesión
    const sesionResponse = await fetch(
      `${BASE_URL}/mesas/qr/MESA-001/sesion`,
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          cantidadPersonas: 2,
          origen: 'QR'
        })
      }
    );
    const sesion = await sesionResponse.json();
    console.log('Sesión creada:', sesion);

    // 2. Confirmar pedido
    const pedidoResponse = await fetch(
      `${BASE_URL}/sesiones/${sesion.id}/pedidos`,
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          itemMenuId: 1,
          cantidad: 2,
          ingresadoPor: 'Cliente'
        })
      }
    );
    const pedido = await pedidoResponse.json();
    console.log('Pedido confirmado:', pedido);

    // 3. Calificar pedido
    const ratingResponse = await fetch(
      `${BASE_URL}/pedidos/${pedido.id}/rating`,
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ puntaje: 1 })
      }
    );
    const rating = await ratingResponse.json();
    console.log('Rating registrado:', rating);

  } catch (error) {
    console.error('Error:', error);
  }
}

flujoCompleto();
```

---

## 🐍 Ejemplos con Python (Requests)

### Obtener Estadísticas

```python
import requests
from datetime import datetime

BASE_URL = "http://localhost:5000/api"

# Obtener ranking de últimos 7 días
def get_ranking(restaurante_id, periodo="7d"):
    response = requests.get(
        f"{BASE_URL}/restaurantes/{restaurante_id}/ranking",
        params={"periodo": periodo}
    )
    return response.json()

# Obtener trending (últimos 30 minutos)
def get_trending(restaurante_id, minutos=30):
    response = requests.get(
        f"{BASE_URL}/restaurantes/{restaurante_id}/trending",
        params={"min": minutos}
    )
    return response.json()

# Obtener recomendados (últimos 30 días)
def get_recomendados(restaurante_id, dias=30):
    response = requests.get(
        f"{BASE_URL}/restaurantes/{restaurante_id}/recomendados",
        params={"dias": dias}
    )
    return response.json()

# Ejemplo de uso
ranking = get_ranking(1, "7d")
print("Top 3 platos más pedidos:")
for item in ranking["items"][:3]:
    print(f"- {item['nombre']}: {item['totalPedidos']} pedidos")

trending = get_trending(1, 30)
print("\nLo que se está pidiendo ahora:")
for item in trending["items"]:
    print(f"- {item['nombre']}: {item['pedidosUltimosMinutos']} pedidos")
```

---

## ⚠️ Manejo de Errores

### Error: Mesa no encontrada

**Request:**
```bash
POST http://localhost:5000/api/mesas/qr/QR-INVALIDO/sesion
```

**Response (400 Bad Request):**
```json
{
  "error": "Mesa no encontrada con QR token: QR-INVALIDO"
}
```

---

### Error: Sesión cerrada

**Request:**
```bash
POST http://localhost:5000/api/sesiones/999/pedidos
```

**Response (400 Bad Request):**
```json
{
  "error": "La sesión ya está cerrada"
}
```

---

### Error: Item inactivo

**Request:**
```bash
POST http://localhost:5000/api/sesiones/1/pedidos
Content-Type: application/json

{
  "itemMenuId": 999
}
```

**Response (400 Bad Request):**
```json
{
  "error": "Item de menú no encontrado: 999"
}
```

---

### Error: Rating inválido

**Request:**
```bash
POST http://localhost:5000/api/pedidos/10/rating
Content-Type: application/json

{
  "puntaje": 5  // Inválido: debe ser -1, 0, o 1
}
```

**Response (400 Bad Request):**
```json
{
  "error": "El puntaje debe ser -1, 0 o 1"
}
```

---

## 💡 Mejores Prácticas

1. **Manejar Errores**
   - Siempre verificar códigos de estado HTTP
   - Leer mensajes de error para debugging

2. **Guardar IDs**
   - Guardar el `id` de la sesión después de crearla
   - Guardar el `id` del pedido después de confirmarlo

3. **UTC en Fechas**
   - Todas las fechas están en UTC
   - Convertir a timezone local en el frontend

4. **Reutilizar Sesiones**
   - El sistema reutiliza sesiones activas automáticamente
   - No es necesario crear múltiples sesiones para la misma mesa

5. **Ratings Opcionales**
   - Los ratings son opcionales
   - Un pedido puede existir sin rating

---

**Última actualización:** Enero 2026
