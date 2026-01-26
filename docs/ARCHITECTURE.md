# 🏗️ Guía de Arquitectura - SGRIA

Documentación detallada de la arquitectura del sistema SGRIA.

## 📐 Arquitectura General

SGRIA sigue una **Arquitectura Limpia (Clean Architecture)** con separación clara de responsabilidades en capas.

```
┌─────────────────────────────────────────┐
│         SGRIA.Api (Presentation)        │
│         - Controllers                    │
│         - Program.cs                     │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      SGRIA.Application (Business)       │
│         - Services                       │
│         - DTOs                           │
│         - Interfaces                      │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│    SGRIA.Infrastructure (Data Access)   │
│         - Repositories                   │
│         - DbContext                      │
│         - Migrations                     │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│        SGRIA.Domain (Entities)         │
│         - Entities                       │
│         - Domain Models                  │
└─────────────────────────────────────────┘
```

## 📦 Estructura de Proyectos

### 1. SGRIA.Domain

**Responsabilidad:** Contiene las entidades del dominio y modelos de negocio.

```
SGRIA.Domain/
├── Entities/
│   ├── Restaurante.cs
│   ├── Mesa.cs
│   ├── SesionMesa.cs
│   ├── ItemMenu.cs
│   ├── SenalPedido.cs
│   ├── SenalRating.cs
│   └── ...
```

**Características:**
- Entidades POCO (Plain Old CLR Objects)
- Sin dependencias externas
- Propiedades de navegación para relaciones
- Validaciones básicas de dominio

**Ejemplo:**
```csharp
public class Mesa
{
    public int Id { get; set; }
    public int RestauranteId { get; set; }
    public string QrToken { get; set; } = default!;
    public bool Activa { get; set; } = true;
    
    // Navegación
    public Restaurante Restaurante { get; set; } = default!;
    public ICollection<SesionMesa> Sesiones { get; set; }
}
```

---

### 2. SGRIA.Application

**Responsabilidad:** Lógica de negocio, DTOs, interfaces de repositorios y servicios.

```
SGRIA.Application/
├── DTOs/
│   ├── SesionMesaDto.cs
│   ├── SenalPedidoDto.cs
│   └── ...
├── Interfaces/
│   ├── ISesionMesaRepository.cs
│   ├── ISenalPedidoRepository.cs
│   └── ...
└── Services/
    ├── SesionMesaService.cs
    ├── SenalPedidoService.cs
    └── ...
```

**Características:**
- **DTOs (Data Transfer Objects):** Objetos para transferencia de datos entre capas
- **Interfaces:** Contratos para repositorios (Dependency Inversion)
- **Services:** Lógica de negocio y orquestación

**Principios:**
- ✅ Depende solo de `SGRIA.Domain`
- ✅ No conoce detalles de infraestructura
- ✅ Contiene toda la lógica de negocio

**Ejemplo de Servicio:**
```csharp
public class SesionMesaService
{
    private readonly ISesionMesaRepository _sesionRepo;
    private readonly IMesaRepository _mesaRepo;

    public async Task<SesionMesaDto> CrearOReutilizarSesionAsync(
        string qrToken, 
        SesionMesaCreateDto? dto, 
        CancellationToken ct)
    {
        // Lógica de negocio: buscar mesa, validar, crear/reutilizar sesión
    }
}
```

---

### 3. SGRIA.Infrastructure

**Responsabilidad:** Implementación de persistencia, acceso a datos, configuración de EF Core.

```
SGRIA.Infrastructure/
├── Persistence/
│   └── AppDbContext.cs
├── Repositories/
│   ├── EfSesionMesaRepository.cs
│   ├── EfSenalPedidoRepository.cs
│   └── ...
└── Migrations/
    └── ...
```

**Características:**
- **DbContext:** Configuración de EF Core con Fluent API
- **Repositories:** Implementaciones concretas de interfaces
- **Migrations:** Migraciones de base de datos

**Configuración de DbContext:**
```csharp
public class AppDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fluent API para configuración de entidades
        modelBuilder.Entity<Mesa>(builder =>
        {
            builder.ToTable("mesas");
            builder.HasIndex(x => x.QrToken).IsUnique();
            // ...
        });
    }
}
```

**Repositorios:**
- Implementan interfaces de `SGRIA.Application`
- Usan `AppDbContext` para acceso a datos
- Métodos asíncronos con `CancellationToken`

---

### 4. SGRIA.Api

**Responsabilidad:** Controladores REST, configuración de la aplicación, middleware.

```
SGRIA.Api/
├── Controllers/
│   ├── MesasQrController.cs
│   ├── SesionesController.cs
│   ├── PedidosController.cs
│   └── RestaurantesController.cs
├── Program.cs
└── appsettings.json
```

**Características:**
- **Controllers:** Endpoints REST
- **Program.cs:** Configuración de servicios, DI, middleware
- **Swagger:** Documentación automática de API

**Configuración de DI:**
```csharp
// Services
builder.Services.AddScoped<SesionMesaService>();
builder.Services.AddScoped<SenalPedidoService>();

// Repositories
builder.Services.AddScoped<ISesionMesaRepository, EfSesionMesaRepository>();
builder.Services.AddScoped<ISenalPedidoRepository, EfSenalPedidoRepository>();
```

---

## 🔄 Flujo de Datos

### Flujo Típico de una Solicitud

```
1. Cliente → HTTP Request
   ↓
2. Controller (SGRIA.Api)
   - Valida formato de request
   - Extrae parámetros
   ↓
3. Service (SGRIA.Application)
   - Ejecuta lógica de negocio
   - Valida reglas de dominio
   ↓
4. Repository (SGRIA.Infrastructure)
   - Accede a base de datos
   - Ejecuta queries
   ↓
5. Entity (SGRIA.Domain)
   - Representa datos del dominio
   ↓
6. Repository → Service → Controller
   - Mapea Entity → DTO
   ↓
7. Controller → HTTP Response
   - Retorna JSON
```

### Ejemplo Concreto: Crear Sesión desde QR

```
POST /api/mesas/qr/MESA-001/sesion
   ↓
MesasQrController.CrearOReutilizarSesion()
   ↓
SesionMesaService.CrearOReutilizarSesionAsync()
   ├─→ MesaRepository.GetByQrTokenAsync()  [Buscar mesa]
   ├─→ Validar mesa activa                  [Lógica negocio]
   ├─→ SesionMesaRepository.GetActivaByMesaIdAsync()  [Buscar sesión activa]
   └─→ SesionMesaRepository.CreateAsync()  [Crear nueva si no existe]
   ↓
Mapear Entity → DTO
   ↓
Return 200 OK con SesionMesaDto
```

---

## 🗄️ Patrón Repository

### ¿Por qué Repository?

- ✅ **Abstracción:** Oculta detalles de acceso a datos
- ✅ **Testeable:** Fácil de mockear en tests
- ✅ **Flexible:** Cambiar implementación sin afectar lógica de negocio
- ✅ **Mantenible:** Código más limpio y organizado

### Estructura

**Interface (Application):**
```csharp
public interface ISesionMesaRepository
{
    Task<SesionMesa?> GetByIdAsync(int id, CancellationToken ct);
    Task<SesionMesa> CreateAsync(SesionMesa sesion, CancellationToken ct);
}
```

**Implementación (Infrastructure):**
```csharp
public class EfSesionMesaRepository : ISesionMesaRepository
{
    private readonly AppDbContext _db;
    
    public async Task<SesionMesa?> GetByIdAsync(int id, CancellationToken ct)
        => await _db.SesionesMesa
            .Include(s => s.Mesa)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
}
```

---

## 🔍 Consultas Optimizadas

### Estrategias de Optimización

1. **Índices en Base de Datos**
   - `QrToken` único
   - Fechas para filtros temporales
   - Foreign keys

2. **Eager Loading**
   ```csharp
   .Include(s => s.Mesa)
   .ThenInclude(m => m.Restaurante)
   ```

3. **Proyecciones**
   - Solo seleccionar campos necesarios
   - Usar DTOs en lugar de entidades completas

4. **Agrupaciones Eficientes**
   ```csharp
   // En lugar de múltiples queries
   var query = from p in _db.SenalesPedido
               group p by p.ItemMenuId into g
               select new { ... };
   ```

---

## 🧪 Testing Strategy

### Capas de Testing

1. **Unit Tests (Services)**
   - Mock de repositorios
   - Validar lógica de negocio

2. **Integration Tests (Repositories)**
   - Base de datos en memoria o test DB
   - Validar queries y mapeos

3. **API Tests (Controllers)**
   - TestClient de ASP.NET Core
   - Validar endpoints completos

---

## 📊 Performance Considerations

### Optimizaciones Implementadas

1. **Índices de Base de Datos**
   - Campos de búsqueda frecuente
   - Foreign keys
   - Campos de fecha para filtros

2. **Consultas Asíncronas**
   - Todos los métodos son `async`
   - Uso de `CancellationToken`

3. **Paginación (Futuro)**
   - Para listados grandes
   - Implementar `Skip()` y `Take()`

4. **Caching (Futuro)**
   - Para estadísticas frecuentes
   - Redis o in-memory cache

---

## 🔐 Seguridad

### Principios Aplicados

1. **Sin Autenticación**
   - Todo es anónimo por diseño
   - No se requiere login

2. **Validación de Entrada**
   - Validaciones en servicios
   - Validaciones en controladores

3. **No Exposición de IDs**
   - Uso de QR tokens en lugar de IDs
   - URLs más seguras

4. **UTC en Fechas**
   - Consistencia temporal
   - Sin problemas de timezone

---

## 🚀 Escalabilidad

### Consideraciones Futuras

1. **Horizontal Scaling**
   - Stateless API
   - Base de datos compartida

2. **Caching Layer**
   - Redis para estadísticas
   - Cache de sesiones activas

3. **Message Queue**
   - Para procesamiento asíncrono
   - Eventos de pedidos

4. **CDN**
   - Para imágenes de items de menú
   - Assets estáticos

---

## 📝 Convenciones de Código

### Nomenclatura

- **Entidades:** PascalCase (`SesionMesa`)
- **DTOs:** PascalCase + `Dto` (`SesionMesaDto`)
- **Repositorios:** `Ef` + Nombre (`EfSesionMesaRepository`)
- **Servicios:** Nombre + `Service` (`SesionMesaService`)
- **Controladores:** Nombre + `Controller` (`MesasQrController`)

### Estructura de Archivos

- Un archivo por clase
- Namespaces por proyecto
- Agrupación lógica (DTOs, Services, etc.)

---

**Última actualización:** Enero 2026
