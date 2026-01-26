namespace SGRIA.Application.DTOs;

public record MesaCreateDto(
    int RestauranteId,
    int Numero,
    int CantidadSillas,
    string QrToken
);

public record MesaUpdateDto(
    int? Numero,
    int? CantidadSillas,
    string? QrToken,
    bool? Activa
);
