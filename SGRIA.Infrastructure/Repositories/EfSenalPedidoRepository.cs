using Microsoft.EntityFrameworkCore;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;
using SGRIA.Infrastructure.Persistence;

namespace SGRIA.Infrastructure.Repositories;

public class EfSenalPedidoRepository : ISenalPedidoRepository
{
    private readonly AppDbContext _db;

    public EfSenalPedidoRepository(AppDbContext db) => _db = db;

    public Task<SenalPedido?> GetByIdAsync(int id, CancellationToken ct)
        => _db.SenalesPedido
              .Include(p => p.ItemMenu)
              .Include(p => p.SesionMesa)
              .ThenInclude(s => s.Mesa)
              .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<SenalPedido> CreateAsync(SenalPedido pedido, CancellationToken ct)
    {
        pedido.FechaHoraConfirmacion = DateTime.UtcNow;
        _db.SenalesPedido.Add(pedido);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(pedido).Reference(p => p.ItemMenu).LoadAsync(ct);
        await _db.Entry(pedido).Reference(p => p.SesionMesa).LoadAsync(ct);
        return pedido;
    }

    public Task<int> CountBySesionAsync(int sesionId, CancellationToken ct)
        => _db.SenalesPedido
              .CountAsync(p => p.SesionMesaId == sesionId, ct);
}
