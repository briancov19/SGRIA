using System.ComponentModel.DataAnnotations;

namespace SGRIA.Application.DTOs;

public record ItemMenuAliasDto(
    int Id,
    int ItemMenuId,
    string ItemMenuNombre,
    string AliasTexto,
    bool Activo
);

public record ItemMenuAliasCreateDto(
    [property: Required][property: Range(1, int.MaxValue)] int ItemMenuId,
    [property: Required][property: MinLength(1)][property: MaxLength(200)] string AliasTexto
);

public record ItemMenuAliasUpdateDto(
    [property: MinLength(1)][property: MaxLength(200)] string? AliasTexto,
    bool? Activo
);
