namespace SGRIA.Application.DTOs;

public record TagRapidoDto(
    int Id,
    string Nombre,
    string? Tipo,
    bool Activo
);

public record TagRapidoCreateDto(
    string Nombre,
    string? Tipo
);

public record TagRapidoUpdateDto(
    string? Nombre,
    string? Tipo,
    bool? Activo
);
