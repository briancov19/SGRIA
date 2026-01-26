namespace SGRIA.Application.DTOs;

public record TagVotoDto(
    int Id,
    int TagId,
    string TagNombre,
    short Valor,
    DateTime FechaHora
);

public record TagVotoCreateDto(
    int TagId,
    short Valor // +1 o -1
);
