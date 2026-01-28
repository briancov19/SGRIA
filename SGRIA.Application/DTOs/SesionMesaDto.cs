using System.ComponentModel.DataAnnotations;

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
    [property: Range(1, 50)] int? CantidadPersonas,
    [property: MaxLength(20)] string? Origen
);
