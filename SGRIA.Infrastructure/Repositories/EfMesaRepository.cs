using Microsoft.EntityFrameworkCore;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfMesaRepository : IMesaRepository
{
    private readonly AppDbContext _db;

    public EfMesaRepository(AppDbContext db) => _db = db;

    public Task<List<Mesa>> GetAllAsync(CancellationToken ct)
        => _db.Mesas
              .Include(m => m.Restaurante)
              .AsNoTracking()
              .OrderByDescending(x => x.Id)
              .ToListAsync(ct);

    public Task<Mesa?> GetByIdAsync(int id, CancellationToken ct)
        => _db.Mesas
              .Include(m => m.Restaurante)
              .AsNoTracking()
              .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Mesa?> GetByQrTokenAsync(string qrToken, CancellationToken ct)
        => _db.Mesas
              .AsNoTracking()
              .Include(m => m.Restaurante)
              .FirstOrDefaultAsync(x => x.QrToken == qrToken && x.Activa, ct);

    public async Task<Mesa> AddAsync(Mesa mesa, CancellationToken ct)
    {
        mesa.FechaModificacion = DateTime.UtcNow;

        _db.Mesas.Add(mesa);
        await _db.SaveChangesAsync(ct);

        return mesa;
    }

    public async Task<Mesa> UpdateAsync(Mesa mesa, CancellationToken ct)
    {
        mesa.FechaModificacion = DateTime.UtcNow;
        _db.Mesas.Update(mesa);
        await _db.SaveChangesAsync(ct);
        return mesa;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var mesa = await _db.Mesas.FindAsync(new object[] { id }, ct);
        if (mesa == null) return false;

        _db.Mesas.Remove(mesa);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task LoadRestauranteAsync(Mesa mesa, CancellationToken ct)
    {
        await _db.Entry(mesa).Reference(m => m.Restaurante).LoadAsync(ct);
    }
}
