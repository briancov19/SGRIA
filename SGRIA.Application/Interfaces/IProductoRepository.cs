using SGRIA.Domain.Entities;

namespace SGRIA.Application.Interfaces;

public interface IProductoRepository
{
    Task<List<Producto>> GetAllAsync(CancellationToken ct);
    Task<Producto?> GetByIdAsync(int id, CancellationToken ct);
    Task<Producto> AddAsync(Producto producto, CancellationToken ct);
}
