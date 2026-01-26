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
    int RestauranteId,
    string Nombre,
    string? Descripcion,
    string? Categoria,
    decimal? Precio,
    string? ImagenUrl
);

public record ItemMenuUpdateDto(
    string? Nombre,
    string? Descripcion,
    string? Categoria,
    decimal? Precio,
    bool? Activo,
    string? ImagenUrl
);
