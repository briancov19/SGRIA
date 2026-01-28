using System.ComponentModel.DataAnnotations;

namespace SGRIA.Application.DTOs;

public record TagVotoDto(
    int Id,
    int TagId,
    string TagNombre,
    short Valor,
    DateTime FechaHora
);

public record TagVotoCreateDto(
    [property: Required][property: Range(1, int.MaxValue)] int TagId,
    [property: Range(-1, 1)] short Valor // +1 o -1
);
