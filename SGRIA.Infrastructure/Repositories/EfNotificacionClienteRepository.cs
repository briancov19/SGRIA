using Microsoft.EntityFrameworkCore;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfNotificacionClienteRepository : INotificacionClienteRepository
{
    private readonly AppDbContext _db;

    public EfNotificacionClienteRepository(AppDbContext db) => _db = db;

    public Task<NotificacionCliente?> GetByIdAsync(int id, CancellationToken ct)
        => _db.Set<NotificacionCliente>()
              .Include(x => x.Mesa) // Importante para que el Service tenga n.Mesa.Numero
              .AsNoTracking()
              .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<NotificacionCliente>> GetActivasAsync(int minutosCorte, CancellationToken ct)
    {
        var fechaLimite = DateTime.UtcNow.AddMinutes(-minutosCorte);

        return await _db.Set<NotificacionCliente>()
              .Include(x => x.Mesa)
              .AsNoTracking()
              .Where(x => !x.Atendida && x.FechaCreacion > fechaLimite)
              .OrderByDescending(x => x.FechaCreacion)
              .ToListAsync(ct);
    }

    public async Task<NotificacionCliente> AddAsync(NotificacionCliente notificacion, CancellationToken ct)
    {
        // El valor por defecto se puede manejar aquí o vía Fluent API con GETUTCDATE()
        notificacion.FechaCreacion = DateTime.UtcNow;
        notificacion.Atendida = false;

        _db.Set<NotificacionCliente>().Add(notificacion);
        await _db.SaveChangesAsync(ct);

        // Cargamos la mesa para que el DTO que devuelve el Service no falle al acceder a n.Mesa.Numero
        await _db.Entry(notificacion).Reference(x => x.Mesa).LoadAsync(ct);

        return notificacion;
    }

    public async Task<bool> MarcarComoAtendidaAsync(int id, CancellationToken ct)
    {
        var item = await _db.Set<NotificacionCliente>()
                            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (item is null) return false;

        item.Atendida = true;
        // En PostgreSQL/EF Core, esto generará un UPDATE solo de la columna NclAtendida
        return await _db.SaveChangesAsync(ct) > 0;
    }
}