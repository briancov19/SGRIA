using System.ComponentModel.DataAnnotations;

namespace SGRIA.Application.DTOs;

public record ItemMenuDto(
    int Id,
    int RestauranteId,
    string RestauranteNombre,
    string Nombre,
    string? Descripcion,
    string? Categoria,
    decimal? Precio,
    bool Activo,
    string? ImagenUrl
);

public record ItemMenuCreateDto(
    [property: Required][property: Range(1, int.MaxValue)] int RestauranteId,
    [property: Required][property: MinLength(1)][property: MaxLength(200)] string Nombre,
    [property: MaxLength(1000)] string? Descripcion,
    [property: MaxLength(100)] string? Categoria,
    [property: Range(0, double.MaxValue)] decimal? Precio,
    [property: MaxLength(500)] string? ImagenUrl
);

public record ItemMenuUpdateDto(
    [property: MinLength(1)][property: MaxLength(200)] string? Nombre,
    [property: MaxLength(1000)] string? Descripcion,
    [property: MaxLength(100)] string? Categoria,
    [property: Range(0, double.MaxValue)] decimal? Precio,
    bool? Activo,
    [property: MaxLength(500)] string? ImagenUrl
);
