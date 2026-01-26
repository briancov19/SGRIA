using SGRIA.Application.DTOs;
using SGRIA.Application.Interfaces;

namespace SGRIA.Application.Services;

public class MesaQrService
{
    private readonly IMesaRepository _mesaRepo;

    public MesaQrService(IMesaRepository mesaRepo)
    {
        _mesaRepo = mesaRepo;
    }

    public async Task<MesaQrDto?> ResolverMesaPorQrAsync(string qrToken, CancellationToken ct)
    {
        var mesa = await _mesaRepo.GetByQrTokenAsync(qrToken, ct);
        if (mesa == null) return null;

        return new MesaQrDto(
            mesa.Id,
            mesa.RestauranteId,
            mesa.Restaurante.Nombre,
            mesa.Numero,
            mesa.Activa
        );
    }
}
