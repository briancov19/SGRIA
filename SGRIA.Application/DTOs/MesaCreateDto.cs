using System.ComponentModel.DataAnnotations;

namespace SGRIA.Application.DTOs;

public record MesaCreateDto(
    [property: Required][property: Range(1, int.MaxValue)] int RestauranteId,
    [property: Required][property: Range(1, int.MaxValue)] int Numero,
    [property: Range(1, 100)] int CantidadSillas,
    [property: Required][property: MinLength(1)][property: MaxLength(100)] string QrToken
);

public record MesaUpdateDto(
    [property: Range(1, int.MaxValue)] int? Numero,
    [property: Range(1, 100)] int? CantidadSillas,
    [property: MinLength(1)][property: MaxLength(100)] string? QrToken,
    bool? Activa
);
