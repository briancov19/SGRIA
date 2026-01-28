using System.ComponentModel.DataAnnotations;

namespace SGRIA.Application.DTOs;

public record TagRapidoDto(
    int Id,
    string Nombre,
    string? Tipo,
    bool Activo
);

public record TagRapidoCreateDto(
    [property: Required][property: MinLength(1)][property: MaxLength(100)] string Nombre,
    [property: MaxLength(50)] string? Tipo
);

public record TagRapidoUpdateDto(
    [property: MinLength(1)][property: MaxLength(100)] string? Nombre,
    [property: MaxLength(50)] string? Tipo,
    bool? Activo
);
