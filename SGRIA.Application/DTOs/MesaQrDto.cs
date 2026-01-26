namespace SGRIA.Application.DTOs;

public record MesaQrDto(
    int MesaId,
    int RestauranteId,
    string RestauranteNombre,
    int NumeroMesa,
    bool Activa
);
