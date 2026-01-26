namespace SGRIA.Application.DTOs;

public record SenalPedidoDto(
    int Id,
    int SesionMesaId,
    int ItemMenuId,
    string ItemMenuNombre,
    int Cantidad,
    DateTime FechaHoraConfirmacion,
    string IngresadoPor,
    decimal? Confianza
);

public record SenalPedidoCreateDto(
    int ItemMenuId,
    int Cantidad = 1,
    string IngresadoPor = "Cliente",
    decimal? Confianza = null
);
