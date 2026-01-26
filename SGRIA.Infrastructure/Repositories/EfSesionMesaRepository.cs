using Microsoft.EntityFrameworkCore;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfSesionMesaRepository : ISesionMesaRepository
{
    private readonly AppDbContext _db;

    public EfSesionMesaRepository(AppDbContext db) => _db = db;

    public Task<SesionMesa?> GetByIdAsync(int id, CancellationToken ct)
        => _db.SesionesMesa
              .Include(s => s.Mesa)
              .ThenInclude(m => m.Restaurante)
              .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<SesionMesa?> GetActivaByMesaIdAsync(int mesaId, CancellationToken ct)
        => _db.SesionesMesa
              .Include(s => s.Mesa)
              .ThenInclude(m => m.Restaurante)
              .Where(s => s.MesaId == mesaId && s.FechaHoraFin == null)
              .OrderByDescending(s => s.FechaHoraInicio)
              .FirstOrDefaultAsync(ct);

    public async Task<SesionMesa> CreateAsync(SesionMesa sesion, CancellationToken ct)
    {
        sesion.FechaHoraInicio = DateTime.UtcNow;
        _db.SesionesMesa.Add(sesion);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(sesion).Reference(s => s.Mesa).LoadAsync(ct);
        await _db.Entry(sesion.Mesa).Reference(m => m.Restaurante).LoadAsync(ct);
        return sesion;
    }

    public async Task<SesionMesa> UpdateAsync(SesionMesa sesion, CancellationToken ct)
    {
        _db.SesionesMesa.Update(sesion);
        await _db.SaveChangesAsync(ct);
        return sesion;
    }

    public async Task<DateTime?> GetUltimaActividadAsync(int sesionId, CancellationToken ct)
    {
        // Obtener la fecha del último pedido de esta sesión
        var ultimoPedido = await _db.SenalesPedido
            .Where(p => p.SesionMesaId == sesionId)
            .OrderByDescending(p => p.FechaHoraConfirmacion)
            .Select(p => (DateTime?)p.FechaHoraConfirmacion)
            .FirstOrDefaultAsync(ct);

        // Si no hay pedidos, usar la fecha de inicio de la sesión
        if (ultimoPedido == null)
        {
            var sesion = await _db.SesionesMesa
                .Where(s => s.Id == sesionId)
                .Select(s => (DateTime?)s.FechaHoraInicio)
                .FirstOrDefaultAsync(ct);
            return sesion;
        }

        return ultimoPedido;
    }

    public async Task<SesionMesa?> GetActivaConActividadRecienteAsync(int mesaId, int minutosTimeout, CancellationToken ct)
    {
        var ahora = DateTime.UtcNow;
        var fechaLimite = ahora.AddMinutes(-minutosTimeout);

        // Buscar sesión activa
        var sesionActiva = await _db.SesionesMesa
            .Include(s => s.Mesa)
            .ThenInclude(m => m.Restaurante)
            .Where(s => s.MesaId == mesaId && s.FechaHoraFin == null)
            .OrderByDescending(s => s.FechaHoraInicio)
            .FirstOrDefaultAsync(ct);

        if (sesionActiva == null)
            return null;

        // Verificar última actividad
        var ultimaActividad = await GetUltimaActividadAsync(sesionActiva.Id, ct);
        
        if (ultimaActividad.HasValue && ultimaActividad.Value >= fechaLimite)
        {
            // Sesión tiene actividad reciente, reutilizar
            return sesionActiva;
        }

        // Sesión muy vieja, cerrarla
        if (ultimaActividad.HasValue)
        {
            sesionActiva.FechaHoraFin = ultimaActividad.Value.AddMinutes(minutosTimeout);
            await UpdateAsync(sesionActiva, ct);
        }

        return null; // No hay sesión válida
    }
}
