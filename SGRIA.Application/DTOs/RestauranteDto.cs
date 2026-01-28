using System.ComponentModel.DataAnnotations;

namespace SGRIA.Application.DTOs;

public record RestauranteDto(
    int Id,
    string Nombre,
    string TimeZone,
    bool Activo,
    DateTime FechaCreacion
);

public record RestauranteCreateDto(
    [property: Required][property: MinLength(1)][property: MaxLength(200)] string Nombre,
    [property: MaxLength(50)] string? TimeZone
);

public record RestauranteUpdateDto(
    [property: MinLength(1)][property: MaxLength(200)] string? Nombre,
    [property: MaxLength(50)] string? TimeZone,
    bool? Activo
);
