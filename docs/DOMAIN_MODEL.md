# 🗃️ Modelo de Dominio - SGRIA

Documentación completa del modelo de datos y entidades del sistema.

## 📊 Diagrama de Entidades

```
┌──────────────┐
│ Restaurante  │
└──────┬───────┘
       │
       ├───┐
       │   │
┌──────▼───▼──────┐      ┌──────────────┐
│     Mesa        │      │  ItemMenu    │
└──────┬──────────┘      └──────┬───────┘
       │                        │
       │                        │
┌──────▼──────────┐      ┌──────▼──────────┐
│  SesionMesa     │      │  SenalPedido    │
└──────┬──────────┘      └──────┬──────────┘
       │                        │
       │                        │
       │                 ┌──────▼──────────┐
       │                 │  SenalRating     │
       │                 └──────────────────┘
       │
       │
┌──────▼──────────────┐
│  VotoTagItemMenu    │
└──────┬──────────────┘
       │
┌──────▼──────────┐
│  TagRapido      │
└─────────────────┘
```

---

## 🏢 Restaurante

**Tabla:** `restaurantes`

**Descripción:** Representa un restaurante en el sistema.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ResId` (PK) | int | ID único del restaurante |
| `ResNombre` | string(200) | Nombre del restaurante |
| `ResTimeZone` | string(50) | Zona horaria (default: "America/Montevideo") |
| `ResActivo` | bool | Si el restaurante está activo |
| `ResFchaCreacion` | DateTime | Fecha de creación (UTC) |

**Relaciones:**
- `1:N` con `Mesa`
- `1:N` con `ItemMenu`

**Índices:**
- `ResNombre`

---

## 🪑 Mesa

**Tabla:** `mesas`

**Descripción:** Representa una mesa física del restaurante con un QR único.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `MesId` (PK) | int | ID único de la mesa |
| `MesResId` (FK) | int | ID del restaurante |
| `MesNumero` | int | Número de mesa |
| `MesCantidadSillas` | int | Cantidad de sillas (default: 4) |
| `MesQrToken` | string(100) | Token QR único e impreso |
| `MesActiva` | bool | Si la mesa está activa |
| `MesFchaModificacion` | DateTime | Última modificación (UTC) |

**Relaciones:**
- `N:1` con `Restaurante`
- `1:N` con `SesionMesa`

**Índices:**
- `MesQrToken` (UNIQUE)
- `(MesResId, MesNumero)` (compuesto)

**Validaciones:**
- `MesQrToken` debe ser único
- `MesNumero` debe ser mayor a 0

---

## 🕐 SesionMesa

**Tabla:** `sesiones_mesa`

**Descripción:** Representa una visita/sesión en una mesa durante un período de tiempo.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `SesId` (PK) | int | ID único de la sesión |
| `SesMesId` (FK) | int | ID de la mesa |
| `SesFchaHoraInicio` | DateTime | Inicio de la sesión (UTC) |
| `SesFchaHoraFin` | DateTime? | Fin de la sesión (nullable) |
| `SesCantidadPersonas` | int? | Cantidad de personas (opcional) |
| `SesOrigen` | string(20) | Origen: "QR", "Manual", "Sistema" |

**Relaciones:**
- `N:1` con `Mesa`
- `1:N` con `SenalPedido`
- `1:N` con `VotoTagItemMenu`

**Índices:**
- `SesMesId`
- `SesFchaHoraInicio`
- `(SesMesId, SesFchaHoraFin)` (compuesto)

**Lógica de Negocio:**
- Si `SesFchaHoraFin` es `null`, la sesión está activa
- Solo puede haber una sesión activa por mesa
- Al crear una nueva sesión, se reutiliza la activa si existe

---

## 🍕 ItemMenu

**Tabla:** `items_menu`

**Descripción:** Representa un plato o bebida del menú del restaurante.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ItmId` (PK) | int | ID único del item |
| `ItmResId` (FK) | int | ID del restaurante |
| `ItmNombre` | string(200) | Nombre del plato |
| `ItmDescripcion` | string(1000) | Descripción (opcional) |
| `ItmCategoria` | string(100) | Categoría (ej: "Pizzas", "Pastas") |
| `ItmPrecio` | decimal(18,2) | Precio (opcional) |
| `ItmActivo` | bool | Si el item está activo |
| `ItmImagenUrl` | string(500) | URL de imagen (opcional) |

**Relaciones:**
- `N:1` con `Restaurante`
- `1:N` con `ItemMenuAlias`
- `1:N` con `SenalPedido`
- `1:N` con `VotoTagItemMenu`

**Índices:**
- `ItmResId`
- `(ItmResId, ItmActivo)` (compuesto)
- `ItmCategoria`

**Validaciones:**
- `ItmNombre` es requerido
- `ItmPrecio` debe ser >= 0 si se proporciona

---

## 📝 ItemMenuAlias

**Tabla:** `items_menu_alias`

**Descripción:** Alias o nombres alternativos para items de menú (ej: "Pizza" = "Pizza Margherita").

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `AliId` (PK) | int | ID único del alias |
| `AliItmId` (FK) | int | ID del item de menú |
| `AliTexto` | string(200) | Texto del alias |
| `AliActivo` | bool | Si el alias está activo |

**Relaciones:**
- `N:1` con `ItemMenu`

**Índices:**
- `AliItmId`
- `(AliTexto, AliActivo)` (compuesto)

---

## 📦 SenalPedido

**Tabla:** `senales_pedido`

**Descripción:** Confirmación de que alguien pidió un item del menú.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PedId` (PK) | int | ID único del pedido |
| `PedSesId` (FK) | int | ID de la sesión de mesa |
| `PedItmId` (FK) | int | ID del item de menú |
| `PedCantidad` | int | Cantidad pedida (default: 1) |
| `PedFchaHoraConfirmacion` | DateTime | Fecha/hora de confirmación (UTC) |
| `PedIngresadoPor` | string(20) | "Cliente", "Mozo", "Sistema" |
| `PedConfianza` | decimal(3,2) | Nivel de confianza 0-1 (opcional, futuro) |

**Relaciones:**
- `N:1` con `SesionMesa`
- `N:1` con `ItemMenu`
- `1:1` con `SenalRating` (opcional)

**Índices:**
- `PedSesId`
- `PedItmId`
- `PedFchaHoraConfirmacion`
- `(PedItmId, PedFchaHoraConfirmacion)` (compuesto)

**Validaciones:**
- `PedCantidad` debe ser > 0
- La sesión debe estar activa (sin `SesFchaHoraFin`)

---

## ⭐ SenalRating

**Tabla:** `senales_rating`

**Descripción:** Rating simple de un pedido: 👍 (1), 😐 (0), 👎 (-1).

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `RatId` (PK) | int | ID único del rating |
| `RatPedId` (FK) | int | ID del pedido (UNIQUE) |
| `RatPuntaje` | short | Puntaje: -1, 0, o 1 |
| `RatFchaHora` | DateTime | Fecha/hora del rating (UTC) |

**Relaciones:**
- `1:1` con `SenalPedido` (obligatorio)

**Índices:**
- `RatPedId` (UNIQUE)
- `(RatPuntaje, RatFchaHora)` (compuesto)

**Validaciones:**
- `RatPuntaje` debe ser -1, 0, o 1
- Un pedido solo puede tener un rating (relación 1:1)
- Si se actualiza, se actualiza `RatFchaHora`

---

## 🏷️ TagRapido

**Tabla:** `tags_rapido`

**Descripción:** Tags rápidos para caracterizar items (ej: "Porción grande", "Pica", "Vegetariano").

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `TagId` (PK) | int | ID único del tag |
| `TagNombre` | string(100) | Nombre del tag |
| `TagTipo` | string(50) | Tipo: "Sabor", "Porcion", "Advertencia", etc. |
| `TagActivo` | bool | Si el tag está activo |

**Relaciones:**
- `1:N` con `VotoTagItemMenu`

**Índices:**
- `(TagNombre, TagActivo)` (compuesto)

---

## 👍 VotoTagItemMenu

**Tabla:** `votos_tag_item_menu`

**Descripción:** Voto de un cliente sobre un tag aplicado a un item de menú.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `VtiId` (PK) | int | ID único del voto |
| `VtiSesId` (FK) | int | ID de la sesión |
| `VtiItmId` (FK) | int | ID del item de menú |
| `VtiTagId` (FK) | int | ID del tag |
| `VtiValor` | short | Valor: +1 o -1 |
| `VtiFchaHora` | DateTime | Fecha/hora del voto (UTC) |

**Relaciones:**
- `N:1` con `SesionMesa`
- `N:1` con `ItemMenu`
- `N:1` con `TagRapido`

**Índices:**
- `(VtiSesId, VtiItmId, VtiTagId)` (compuesto)
- `VtiItmId`

---

## 🔗 Relaciones Resumen

| Entidad A | Relación | Entidad B | Cardinalidad |
|-----------|----------|-----------|--------------|
| Restaurante | tiene | Mesa | 1:N |
| Restaurante | tiene | ItemMenu | 1:N |
| Mesa | tiene | SesionMesa | 1:N |
| SesionMesa | tiene | SenalPedido | 1:N |
| ItemMenu | tiene | SenalPedido | 1:N |
| SenalPedido | tiene | SenalRating | 1:1 |
| ItemMenu | tiene | ItemMenuAlias | 1:N |
| SesionMesa | tiene | VotoTagItemMenu | 1:N |
| ItemMenu | tiene | VotoTagItemMenu | 1:N |
| TagRapido | tiene | VotoTagItemMenu | 1:N |

---

## 📊 Consultas Comunes

### Ranking de Pedidos

```sql
SELECT 
    itm.ItmId,
    itm.ItmNombre,
    COUNT(ped.PedId) as TotalPedidos,
    SUM(ped.PedCantidad) as TotalCantidad
FROM senales_pedido ped
JOIN items_menu itm ON ped.PedItmId = itm.ItmId
WHERE ped.PedFchaHoraConfirmacion >= @fechaDesde
  AND ped.PedFchaHoraConfirmacion <= @fechaHasta
GROUP BY itm.ItmId, itm.ItmNombre
ORDER BY TotalPedidos DESC
```

### Trending (Últimos X minutos)

```sql
SELECT 
    itm.ItmId,
    itm.ItmNombre,
    COUNT(ped.PedId) as PedidosUltimosMinutos,
    MAX(ped.PedFchaHoraConfirmacion) as UltimoPedido
FROM senales_pedido ped
JOIN items_menu itm ON ped.PedItmId = itm.ItmId
WHERE ped.PedFchaHoraConfirmacion >= NOW() - INTERVAL '@minutos minutes'
GROUP BY itm.ItmId, itm.ItmNombre
ORDER BY PedidosUltimosMinutos DESC
```

### Platos Más Recomendados

```sql
SELECT 
    itm.ItmId,
    itm.ItmNombre,
    AVG(rat.RatPuntaje) as PromedioRating,
    COUNT(rat.RatId) as TotalRatings
FROM senales_rating rat
JOIN senales_pedido ped ON rat.RatPedId = ped.PedId
JOIN items_menu itm ON ped.PedItmId = itm.ItmId
WHERE rat.RatFchaHora >= @fechaDesde
  AND rat.RatFchaHora <= @fechaHasta
GROUP BY itm.ItmId, itm.ItmNombre
HAVING COUNT(rat.RatId) >= 5
ORDER BY PromedioRating DESC
```

---

## 🔍 Índices Críticos

### Para Performance

1. **Búsqueda por QR Token**
   ```sql
   CREATE UNIQUE INDEX idx_mesas_qr_token ON mesas(MesQrToken);
   ```

2. **Filtros Temporales**
   ```sql
   CREATE INDEX idx_pedidos_fecha ON senales_pedido(PedFchaHoraConfirmacion);
   CREATE INDEX idx_ratings_fecha ON senales_rating(RatFchaHora);
   ```

3. **Agrupaciones**
   ```sql
   CREATE INDEX idx_pedidos_item_fecha ON senales_pedido(PedItmId, PedFchaHoraConfirmacion);
   ```

---

**Última actualización:** Enero 2026
