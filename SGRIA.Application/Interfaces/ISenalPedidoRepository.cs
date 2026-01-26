using SGRIA.Domain.Entities;

namespace SGRIA.Application.Interfaces;

public interface ISenalPedidoRepository
{
    Task<SenalPedido?> GetByIdAsync(int id, CancellationToken ct);
    Task<SenalPedido> CreateAsync(SenalPedido pedido, CancellationToken ct);
}
