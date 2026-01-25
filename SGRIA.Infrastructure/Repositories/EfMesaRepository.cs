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
              .AsNoTracking()
              .OrderByDescending(x => x.Id)
              .ToListAsync(ct);

    public Task<Mesa?> GetByIdAsync(int id, CancellationToken ct)
        => _db.Mesas
              .AsNoTracking()
              .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Mesa> AddAsync(Mesa mesa, CancellationToken ct)
    {
        mesa.FechaModificacion = DateTime.UtcNow;

        _db.Mesas.Add(mesa);
        await _db.SaveChangesAsync(ct);

        return mesa;
    }
}
