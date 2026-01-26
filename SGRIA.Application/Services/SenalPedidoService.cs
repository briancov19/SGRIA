using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class SenalPedidoService
{
    private readonly ISenalPedidoRepository _pedidoRepo;
    private readonly ISesionMesaRepository _sesionRepo;
    private readonly IItemMenuRepository _itemRepo;

    public SenalPedidoService(
        ISenalPedidoRepository pedidoRepo,
        ISesionMesaRepository sesionRepo,
        IItemMenuRepository itemRepo)
    {
        _pedidoRepo = pedidoRepo;
        _sesionRepo = sesionRepo;
        _itemRepo = itemRepo;
    }

    public async Task<SenalPedidoDto> ConfirmarPedidoAsync(
        int sesionId,
        SenalPedidoCreateDto dto,
        CancellationToken ct)
    {
        // Validar sesión
        var sesion = await _sesionRepo.GetByIdAsync(sesionId, ct);
        if (sesion == null)
        {
            throw new ArgumentException($"Sesión no encontrada: {sesionId}");
        }

        if (sesion.FechaHoraFin.HasValue)
        {
            throw new InvalidOperationException("La sesión ya está cerrada");
        }

        // Validar item de menú
        var item = await _itemRepo.GetByIdAsync(dto.ItemMenuId, ct);
        if (item == null)
        {
            throw new ArgumentException($"Item de menú no encontrado: {dto.ItemMenuId}");
        }

        if (!item.Activo)
        {
            throw new InvalidOperationException("El item de menú no está activo");
        }

        // Crear señal de pedido
        var pedido = new SenalPedido
        {
            SesionMesaId = sesionId,
            ItemMenuId = dto.ItemMenuId,
            Cantidad = dto.Cantidad > 0 ? dto.Cantidad : 1,
            IngresadoPor = dto.IngresadoPor ?? "Cliente",
            Confianza = dto.Confianza,
            FechaHoraConfirmacion = DateTime.UtcNow
        };

        var pedidoCreado = await _pedidoRepo.CreateAsync(pedido, ct);

        return new SenalPedidoDto(
            pedidoCreado.Id,
            pedidoCreado.SesionMesaId,
            pedidoCreado.ItemMenuId,
            pedidoCreado.ItemMenu.Nombre,
            pedidoCreado.Cantidad,
            pedidoCreado.FechaHoraConfirmacion,
            pedidoCreado.IngresadoPor,
            pedidoCreado.Confianza
        );
    }

    public async Task<SenalPedidoDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var pedido = await _pedidoRepo.GetByIdAsync(id, ct);
        if (pedido == null) return null;

        return new SenalPedidoDto(
            pedido.Id,
            pedido.SesionMesaId,
            pedido.ItemMenuId,
            pedido.ItemMenu.Nombre,
            pedido.Cantidad,
            pedido.FechaHoraConfirmacion,
            pedido.IngresadoPor,
            pedido.Confianza
        );
    }
}
