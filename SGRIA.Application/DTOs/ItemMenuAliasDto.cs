namespace SGRIA.Application.DTOs;

public record ItemMenuAliasDto(
    int Id,
    int ItemMenuId,
    string ItemMenuNombre,
    string AliasTexto,
    bool Activo
);

public record ItemMenuAliasCreateDto(
    int ItemMenuId,
    string AliasTexto
);

public record ItemMenuAliasUpdateDto(
    string? AliasTexto,
    bool? Activo
);
