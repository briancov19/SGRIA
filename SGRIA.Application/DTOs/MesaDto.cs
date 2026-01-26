namespace SGRIA.Application.DTOs;

public record MesaDto(
    int Id,
    int RestauranteId,
    string RestauranteNombre,
    int Numero,
    int CantidadSillas,
    string QrToken,
    bool Activa,
    DateTime FechaModificacion
);
