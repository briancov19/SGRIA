# 🛡️ Sistema de Confianza y Anti-Abuso - SGRIA

Documentación completa del sistema de confianza y mitigación de abuso implementado en SGRIA.

## 📋 Índice

1. [Visión General](#visión-general)
2. [Arquitectura del Sistema](#arquitectura-del-sistema)
3. [Participantes Anónimos](#participantes-anónimos)
4. [Rate Limiting](#rate-limiting)
5. [Cálculo de Confianza](#cálculo-de-confianza)
6. [Protección del QR](#protección-del-qr)
7. [Configuración](#configuración)
8. [Uso en Frontend](#uso-en-frontend)

---

## 🎯 Visión General

El sistema de confianza y anti-abuso de SGRIA está diseñado para:

- ✅ **Reducir spam de pedidos** - Limitar confirmaciones falsas
- ✅ **Reducir spam de ratings** - Prevenir votos maliciosos repetidos
- ✅ **Mitigar abuso desde fuera del local** - Dificultar el uso de QRs fotografiados
- ✅ **Calcular confianza automática** - Asignar un score de confianza (0.0-1.0) a cada pedido
- ✅ **Filtrar contenido sospechoso** - Excluir pedidos de baja confianza del feed público

**Principio de diseño:** Todo es anónimo, sin login, sin PII (Personally Identifiable Information).

---

## 🏗️ Arquitectura del Sistema

El sistema funciona en **3 capas**:

### 1. Participante Anónimo por Sesión

Cada dispositivo que interactúa con el sistema se identifica mediante un hash único calculado a partir de:
- `X-Client-Id` (GUID generado en frontend, guardado en localStorage)
- `ServerSalt` (configuración del servidor)

**No se almacena:**
- ❌ IP completa
- ❌ User-Agent completo
- ❌ Cualquier dato que identifique al usuario

**Se almacena:**
- ✅ Hash SHA256 del dispositivo (`DeviceHash`)
- ✅ Relación entre sesión y dispositivo (`SesionParticipante`)
- ✅ Última actividad del participante

### 2. Rate Limiting a Nivel de Base de Datos

Sin depender de middleware externo, se aplican reglas de negocio:

- **Límite de pedidos:** Máximo 10 pedidos por participante cada 10 minutos
- **Límite de ratings:** Máximo 10 actualizaciones de rating por participante cada 10 minutos

Si se excede el límite, se devuelve `429 Too Many Requests`.

### 3. Score de Confianza Automático

Cada pedido recibe un score de confianza (0.0 a 1.0) calculado automáticamente basado en heurísticas:

- **Sesión "fresh":** +20% si el participante se unió en los últimos 5 minutos
- **Actividad reciente:** +10% si tiene actividad en los últimos 10 minutos
- **Actividad humana razonable:** -30% si hay más de 10 pedidos por minuto
- **Distancia de sesión:** -20% si la sesión tiene más de 24 horas
- **Sesión cerrada:** Confianza mínima (0.1) si la sesión está cerrada

---

## 👤 Participantes Anónimos

### Entidades

#### `AnonDevice`
Representa un dispositivo anónimo identificado por un hash único.

```csharp
public class AnonDevice
{
    public int Id { get; set; }
    public string DeviceHash { get; set; }  // SHA256(clientId + serverSalt)
    public DateTime FechaCreacion { get; set; }
}
```

#### `SesionParticipante`
Relación entre una sesión de mesa y un dispositivo anónimo.

```csharp
public class SesionParticipante
{
    public int Id { get; set; }
    public int SesionMesaId { get; set; }
    public int AnonDeviceId { get; set; }
    public DateTime FechaHoraJoin { get; set; }
    public DateTime UltimaActividad { get; set; }
}
```

### Flujo de Identificación

1. **Frontend genera `X-Client-Id`** (GUID) y lo guarda en `localStorage`
2. **Frontend envía `X-Client-Id`** en el header `X-Client-Id` en cada request
3. **Backend calcula `DeviceHash`** = SHA256(`X-Client-Id` + `ServerSalt`)
4. **Backend busca o crea `AnonDevice`** con ese hash
5. **Backend asocia `SesionParticipante`** cuando se crea/reutiliza sesión
6. **Backend actualiza `UltimaActividad`** en cada acción

### Si `X-Client-Id` no está presente

- **En `POST /api/mesas/qr/{qrToken}/sesion`:** El backend genera un nuevo GUID y lo devuelve en el header `X-Client-Id`. El frontend debe guardarlo (p. ej. `localStorage`) para usarlo en pedidos, ratings y tags.
- **En `POST /api/sesiones/{token}/pedidos`, `POST /api/pedidos/{id}/rating`, `POST /api/sesiones/{token}/items/{id}/tags`:** `X-Client-Id` es **obligatorio**. Si falta, se responde `400 Bad Request`. Es necesario para rate limiting, actividad reciente y participación en la sesión.

---

## ⏱️ Rate Limiting

### Reglas Implementadas

| Acción | Límite | Ventana | Código de Error |
|--------|--------|---------|-----------------|
| Crear pedido | 10 pedidos | 10 minutos | `429 Too Many Requests` |
| Actualizar rating | 10 actualizaciones | 10 minutos | `429 Too Many Requests` |

### Validación

La validación se realiza **antes** de crear/actualizar el recurso:

```csharp
// En SenalPedidoService
await _rateLimitService.ValidarLimitePedidosAsync(participante.Id, ct);

// En SenalRatingService (crear y actualizar)
await _rateLimitService.ValidarLimiteRatingsAsync(participante.Id, ct);

// En TagVotoService
await _rateLimitService.ValidarLimiteTagVotosAsync(participante.Id, ct);
```

### Mensaje de Error

```json
{
  "error": "Límite de pedidos excedido. Máximo 10 pedidos cada 10 minutos."
}
```

---

## 📊 Cálculo de Confianza

### Heurísticas Implementadas

El score de confianza se calcula usando las siguientes señales:

#### 1. Sesión "Fresh" (+20%)
Si el participante se unió a la sesión en los últimos 5 minutos:
```csharp
if (minutosDesdeJoin <= 5)
    confianza += 0.2m;
```

#### 2. Actividad Reciente del Participante (+10%)
Si el participante tiene actividad en los últimos 10 minutos:
```csharp
if (minutosDesdeActividad <= 10)
    confianza += 0.1m;
```

#### 3. Actividad Humana Razonable (-30% o -10%)
Si hay muchos pedidos en poco tiempo:
```csharp
var pedidosPorMinuto = totalPedidosEnSesion / minutosDesdeInicioSesion;
if (pedidosPorMinuto > 10)
    confianza -= 0.3m;  // Muy sospechoso
else if (pedidosPorMinuto > 5)
    confianza -= 0.1m;  // Sospechoso
```

#### 4. Distancia de Sesión (-20% o -10%)
Si la sesión tiene mucho tiempo abierta:
```csharp
if (horasDesdeInicio > 24)
    confianza -= 0.2m;
else if (horasDesdeInicio > 12)
    confianza -= 0.1m;
```

#### 5. Sesión Cerrada (0.1)
Si la sesión está cerrada, confianza mínima:
```csharp
if (sesion.FechaHoraFin.HasValue)
    confianza = 0.1m;
```

### Rango Final

El score final se asegura de estar entre **0.0 y 1.0**:
```csharp
return Math.Max(0.0m, Math.Min(1.0m, confianza));
```

### Uso del Score

- **Almacenado en `PedConfianza`** en cada `SenalPedido`
- **Filtrado en feed público:** Por defecto, solo se muestran pedidos con `confianza >= 0.3`
- **Filtrado en estadísticas:** Opcional mediante parámetro `minConfianza`

---

## 🔒 Protección del QR

### Separación: "Mirar Feed" vs "Crear Señales"

#### GET Feed (Siempre Permitido)
- `GET /api/mesas/qr/{qrToken}/feed` - **Siempre permitido**
- No requiere `X-Client-Id`
- No valida actividad reciente
- Útil para que cualquiera pueda ver el feed

#### POST Pedidos / POST Rating (Protección Activa)
- `POST /api/sesiones/{sesionId}/pedidos` - **Requiere actividad reciente**
- `POST /api/pedidos/{pedidoId}/rating` - **Requiere actividad reciente**

**Validación:**
```csharp
var minutosDesdeActividad = (DateTime.UtcNow - participante.UltimaActividad).TotalMinutes;
if (minutosDesdeActividad > 10)
{
    throw new InvalidOperationException(
        "Sesión no válida o expirada. Por favor, escanea el QR nuevamente.");
}
```

**Código de Error:** `409 Conflict`

### Flujo Recomendado

1. Usuario escanea QR → `POST /api/mesas/qr/{qrToken}/sesion`
2. Frontend guarda `X-Client-Id` de la respuesta
3. Usuario puede ver feed → `GET /api/mesas/qr/{qrToken}/feed` (sin restricciones)
4. Usuario confirma pedido → `POST /api/sesiones/{sesionId}/pedidos` (con `X-Client-Id`)
5. Si pasan más de 10 minutos sin actividad, debe re-escaneear el QR

---

## ⚙️ Configuración

### appsettings.json

```json
{
  "AntiAbuse": {
    "ServerSalt": "SGRIA-DEFAULT-SALT-CHANGE-IN-PRODUCTION",
    "MaxPedidosPorVentana": 10,
    "MaxRatingsPorVentana": 10,
    "VentanaMinutos": 10,
    "MinConfianzaFeedPublico": 0.3
  }
}
```

### Parámetros

| Parámetro | Descripción | Valor por Defecto |
|-----------|-------------|-------------------|
| `ServerSalt` | Salt del servidor para calcular DeviceHash | `"SGRIA-DEFAULT-SALT-CHANGE-IN-PRODUCTION"` |
| `MaxPedidosPorVentana` | Máximo de pedidos por participante en la ventana | `10` |
| `MaxRatingsPorVentana` | Máximo de ratings por participante en la ventana | `10` |
| `VentanaMinutos` | Duración de la ventana de rate limiting (minutos) | `10` |
| `MinConfianzaFeedPublico` | Confianza mínima para mostrar en feed público | `0.3` |

**⚠️ IMPORTANTE:** Cambiar `ServerSalt` en producción a un valor seguro y único.

---

## 💻 Uso en Frontend

### 1. Generar y Guardar X-Client-Id

```javascript
// Al iniciar la app
function getOrCreateClientId() {
    let clientId = localStorage.getItem('x-client-id');
    if (!clientId) {
        clientId = crypto.randomUUID(); // o usar una librería de UUID
        localStorage.setItem('x-client-id', clientId);
    }
    return clientId;
}
```

### 2. Enviar en Requests

```javascript
// Ejemplo con Fetch API
async function confirmarPedido(sesionId, itemMenuId) {
    const clientId = getOrCreateClientId();
    
    const response = await fetch(`/api/sesiones/${sesionId}/pedidos`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Client-Id': clientId  // ⬅️ Importante
        },
        body: JSON.stringify({
            itemMenuId: itemMenuId,
            cantidad: 1
        })
    });
    
    // Si el servidor devuelve un nuevo X-Client-Id, guardarlo
    const newClientId = response.headers.get('X-Client-Id');
    if (newClientId) {
        localStorage.setItem('x-client-id', newClientId);
    }
    
    if (response.status === 429) {
        const error = await response.json();
        alert(`Límite excedido: ${error.error}`);
        return;
    }
    
    if (response.status === 409) {
        const error = await response.json();
        alert(`Sesión expirada: ${error.error}`);
        // Pedir re-escaneear QR
        return;
    }
    
    return await response.json();
}
```

### 3. Manejar Errores

```javascript
// 429 Too Many Requests - Rate limiting
if (response.status === 429) {
    // Mostrar mensaje al usuario
    // Esperar antes de reintentar
}

// 409 Conflict - Sesión expirada
if (response.status === 409) {
    // Pedir re-escaneear QR
    // Limpiar sesión local
}
```

---

## 📈 Estadísticas con Filtrado por Confianza

### Endpoints Actualizados

Todos los endpoints de estadísticas ahora aceptan un parámetro opcional `minConfianza`:

- `GET /api/restaurantes/{id}/ranking?periodo=7d&minConfianza=0.5`
- `GET /api/restaurantes/{id}/trending?min=30&minConfianza=0.5`
- `GET /api/restaurantes/{id}/recomendados?dias=30&minConfianza=0.5`

### Feed Público

El feed público (`GET /api/mesas/qr/{qrToken}/feed`) **siempre** usa `minConfianza = 0.3` por defecto (configurable).

---

## 🔍 Ejemplos de Cálculo de Confianza

### Caso 1: Usuario Normal
- Se une a sesión hace 2 minutos → +20%
- Tiene actividad reciente → +10%
- 3 pedidos en 10 minutos (0.3 pedidos/min) → Sin penalización
- Sesión abierta hace 15 minutos → Sin penalización
- **Confianza final: ~0.8 (80%)**

### Caso 2: Bot/Spam
- Se une a sesión hace 30 minutos → Sin bonus
- Sin actividad reciente → Sin bonus
- 50 pedidos en 1 minuto (50 pedidos/min) → -30%
- Sesión abierta hace 2 horas → -10%
- **Confianza final: ~0.1 (10%)**

### Caso 3: QR Fotografiado (Uso Remoto)
- Se une a sesión hace 2 horas → Sin bonus
- Sin actividad reciente (última actividad hace 2 horas) → Sin bonus
- Intenta crear pedido → Validación falla: "Sesión no válida o expirada"
- **No se crea el pedido**

---

## 🧪 Testing

### Tests Recomendados

1. **Rate Limiting:**
   - Crear 11 pedidos en 10 minutos → Debe fallar con 429

2. **Confianza Calculada:**
   - Verificar que pedidos normales tengan confianza > 0.5
   - Verificar que pedidos sospechosos tengan confianza < 0.3

3. **Bloqueo por Sesión Expirada:**
   - Crear sesión, esperar 11 minutos, intentar crear pedido → Debe fallar con 409

---

## 📝 Notas de Implementación

- ✅ **MVP Completo:** Sin autenticación, sin complejidad, sin servicios externos
- ✅ **Solo EF Core + PostgreSQL:** Todo se maneja en la base de datos
- ✅ **Anónimo:** No se almacena PII
- ✅ **Escalable:** Los índices están optimizados para consultas rápidas

---

**Última actualización:** Enero 2026
