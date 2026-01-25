using Microsoft.EntityFrameworkCore;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfProductoRepository : IProductoRepository
{
    private readonly AppDbContext _db;

    public EfProductoRepository(AppDbContext db) => _db = db;

    public Task<List<Producto>> GetAllAsync(CancellationToken ct)
        => _db.Productos
              .AsNoTracking()
              .OrderByDescending(x => x.Id)
              .ToListAsync(ct);

    public Task<Producto?> GetByIdAsync(int id, CancellationToken ct)
        => _db.Productos
              .AsNoTracking()
              .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Producto> AddAsync(Producto producto, CancellationToken ct)
    {
        producto.CreatedAt = DateTime.UtcNow;

        _db.Productos.Add(producto);
        await _db.SaveChangesAsync(ct);

        return producto;
    }
}
