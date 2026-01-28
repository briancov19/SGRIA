using System.ComponentModel.DataAnnotations;

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
    [property: Required][property: Range(1, int.MaxValue)] int ItemMenuId,
    [property: Range(1, int.MaxValue)] int Cantidad = 1,
    [property: MaxLength(20)] string IngresadoPor = "Cliente",
    [property: Range(0, 1)] decimal? Confianza = null
);
