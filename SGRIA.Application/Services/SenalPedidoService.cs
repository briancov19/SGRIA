using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;
using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

public class SenalPedidoService
{
    private readonly ISenalPedidoRepository _pedidoRepo;
    private readonly ISesionMesaRepository _sesionRepo;
    private readonly IItemMenuRepository _itemRepo;
    private readonly AnonDeviceService _deviceService;
    private readonly ISesionParticipanteRepository _participanteRepo;
    private readonly ConfianzaService _confianzaService;
    private readonly RateLimitService _rateLimitService;
    private readonly SesionPublicaService _sesionPublicaService;

    public SenalPedidoService(
        ISenalPedidoRepository pedidoRepo,
        ISesionMesaRepository sesionRepo,
        IItemMenuRepository itemRepo,
        AnonDeviceService deviceService,
        ISesionParticipanteRepository participanteRepo,
        ConfianzaService confianzaService,
        RateLimitService rateLimitService,
        SesionPublicaService sesionPublicaService)
    {
        _pedidoRepo = pedidoRepo;
        _sesionRepo = sesionRepo;
        _itemRepo = itemRepo;
        _deviceService = deviceService;
        _participanteRepo = participanteRepo;
        _confianzaService = confianzaService;
        _rateLimitService = rateLimitService;
        _sesionPublicaService = sesionPublicaService;
    }

    /// <summary>
    /// Confirma un pedido usando el token público de la sesión.
    /// </summary>
    public async Task<SenalPedidoDto> ConfirmarPedidoPorTokenAsync(
        string sesPublicToken,
        string? clientId,
        SenalPedidoCreateDto dto,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("X-Client-Id es requerido.");

        // Validar sesión activa y no expirada usando token público
        var sesion = await _sesionPublicaService.GetActiveSessionByPublicTokenAsync(sesPublicToken, ct);

        var device = await _deviceService.GetOrCreateDeviceAsync(clientId!, ct);
        var participante = await _participanteRepo.GetActivoBySesionAndDeviceHashAsync(
            sesion.Id, 
            device.DeviceHash, 
            ct);

        if (participante == null)
            throw new InvalidOperationException("Debes escanear el QR de la mesa para unirte a la sesión antes de confirmar pedidos.");

        // Validar que la última actividad sea reciente (máximo 10 minutos)
        var minutosDesdeActividad = (DateTime.UtcNow - participante.UltimaActividad).TotalMinutes;
        if (minutosDesdeActividad > 10)
        {
            throw new InvalidOperationException(
                "Sesión no válida o expirada. Por favor, escanea el QR nuevamente.");
        }

        await _rateLimitService.ValidarLimitePedidosAsync(participante.Id, ct);
        participante.UltimaActividad = DateTime.UtcNow;
        await _participanteRepo.UpdateAsync(participante, ct);

        // Validar item de menú
        var item = await _itemRepo.GetByIdAsync(dto.ItemMenuId, ct);
        if (item == null)
            throw new ArgumentException("Item de menú no encontrado.");

        if (!item.Activo)
            throw new InvalidOperationException("El item de menú no está activo.");

        if (item.RestauranteId != sesion.Mesa.RestauranteId)
            throw new InvalidOperationException("El item de menú no pertenece al restaurante de esta sesión.");

        // Calcular confianza
        // Contar pedidos de la sesión (cargar si no están cargados)
        var totalPedidosEnSesion = await _pedidoRepo.CountBySesionAsync(sesion.Id, ct);
        var confianza = _confianzaService.CalcularConfianza(
            participante,
            sesion,
            totalPedidosEnSesion,
            DateTime.UtcNow);

        // Crear señal de pedido
        var pedido = new SenalPedido
        {
            SesionMesaId = sesion.Id,
            ItemMenuId = dto.ItemMenuId,
            Cantidad = dto.Cantidad > 0 ? dto.Cantidad : 1,
            IngresadoPor = dto.IngresadoPor ?? "Cliente",
            Confianza = confianza, // Usar confianza calculada
            FechaHoraConfirmacion = DateTime.UtcNow
        };

        var pedidoCreado = await _pedidoRepo.CreateAsync(pedido, ct);
        
        // Actualizar última actividad de la sesión (touch)
        await _sesionPublicaService.TouchSessionAsync(sesion.Id, ct);

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
