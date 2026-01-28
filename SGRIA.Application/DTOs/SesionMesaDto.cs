namespace SGRIA.Application.DTOs;

/// <summary>
/// DTO interno para sesión (incluye IDs). Para uso administrativo.
/// </summary>
public record SesionMesaDto(
    int Id,
    int MesaId,
    string SesPublicToken,
    DateTime FechaHoraInicio,
    DateTime? FechaHoraFin,
    DateTime FechaHoraUltActividad,
    int? CantidadPersonas,
    string? Origen
);

public record SesionMesaCreateDto(
    int? CantidadPersonas,
    string? Origen
);
