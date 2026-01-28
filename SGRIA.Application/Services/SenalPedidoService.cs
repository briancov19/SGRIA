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
        // Validar sesión activa y no expirada usando token público
        var sesion = await _sesionPublicaService.GetActiveSessionByPublicTokenAsync(sesPublicToken, ct);

        // Validar actividad reciente del participante (protección QR)
        SesionParticipante? participante = null;
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            var device = await _deviceService.GetOrCreateDeviceAsync(clientId, ct);
            participante = await _participanteRepo.GetActivoBySesionAndDeviceHashAsync(
                sesion.Id, 
                device.DeviceHash, 
                ct);
            
            if (participante != null)
            {
                // Validar que la última actividad sea reciente (máximo 10 minutos)
                var minutosDesdeActividad = (DateTime.UtcNow - participante.UltimaActividad).TotalMinutes;
                if (minutosDesdeActividad > 10)
                {
                    throw new InvalidOperationException(
                        "Sesión no válida o expirada. Por favor, escanea el QR nuevamente.");
                }

                // Validar rate limiting
                await _rateLimitService.ValidarLimitePedidosAsync(participante.Id, ct);
                
                // Actualizar última actividad
                participante.UltimaActividad = DateTime.UtcNow;
                await _participanteRepo.UpdateAsync(participante, ct);
            }
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

        // Validar que el item pertenece al mismo restaurante que la mesa/sesión
        if (item.RestauranteId != sesion.Mesa.RestauranteId)
        {
            throw new InvalidOperationException(
                $"El item de menú no pertenece al restaurante de esta sesión. " +
                $"Item restaurante: {item.RestauranteId}, Sesión restaurante: {sesion.Mesa.RestauranteId}");
        }

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
