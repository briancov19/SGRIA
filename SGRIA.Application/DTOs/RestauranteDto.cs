namespace SGRIA.Application.DTOs;

public record RestauranteDto(
    int Id,
    string Nombre,
    string TimeZone,
    bool Activo,
    DateTime FechaCreacion
);

public record RestauranteCreateDto(
    string Nombre,
    string? TimeZone
);

public record RestauranteUpdateDto(
    string? Nombre,
    string? TimeZone,
    bool? Activo
);
