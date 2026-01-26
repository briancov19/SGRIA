namespace SGRIA.Application.DTOs;

public record SesionMesaDto(
    int Id,
    int MesaId,
    DateTime FechaHoraInicio,
    DateTime? FechaHoraFin,
    int? CantidadPersonas,
    string? Origen
);

public record SesionMesaCreateDto(
    int? CantidadPersonas,
    string? Origen
);
