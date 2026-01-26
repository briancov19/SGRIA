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
}
