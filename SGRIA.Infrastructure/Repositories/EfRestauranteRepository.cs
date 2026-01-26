using Microsoft.EntityFrameworkCore;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfRestauranteRepository : IRestauranteRepository
{
    private readonly AppDbContext _db;

    public EfRestauranteRepository(AppDbContext db) => _db = db;

    public Task<List<Restaurante>> GetAllAsync(CancellationToken ct)
        => _db.Restaurantes
              .AsNoTracking()
              .OrderBy(r => r.Nombre)
              .ToListAsync(ct);

    public Task<Restaurante?> GetByIdAsync(int id, CancellationToken ct)
        => _db.Restaurantes
              .AsNoTracking()
              .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Restaurante> AddAsync(Restaurante restaurante, CancellationToken ct)
    {
        restaurante.FechaCreacion = DateTime.UtcNow;
        _db.Restaurantes.Add(restaurante);
        await _db.SaveChangesAsync(ct);
        return restaurante;
    }

    public async Task<Restaurante> UpdateAsync(Restaurante restaurante, CancellationToken ct)
    {
        _db.Restaurantes.Update(restaurante);
        await _db.SaveChangesAsync(ct);
        return restaurante;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var restaurante = await _db.Restaurantes.FindAsync(new object[] { id }, ct);
        if (restaurante == null) return false;

        _db.Restaurantes.Remove(restaurante);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
