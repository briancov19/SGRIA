namespace SGRIA.Application.DTOs;

/// <summary>
/// DTO público para sesión de mesa. Expone solo el token público, no IDs internos.
/// </summary>
public record SesionPublicaDto(
    string SesPublicToken,
    DateTime FechaHoraInicio,
    DateTime? FechaHoraFin,
    int? CantidadPersonas,
    string? Origen
);
