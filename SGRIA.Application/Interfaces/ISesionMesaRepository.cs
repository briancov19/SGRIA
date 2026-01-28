using SGRIA.Domain.Entities;

namespace SGRIA.Application.Interfaces;

public interface ISesionMesaRepository
{
    Task<SesionMesa?> GetByIdAsync(int id, CancellationToken ct);
    Task<SesionMesa?> GetByPublicTokenAsync(string sesPublicToken, CancellationToken ct);
    Task<SesionMesa?> GetActivaByMesaIdAsync(int mesaId, CancellationToken ct);
    Task<SesionMesa> CreateAsync(SesionMesa sesion, CancellationToken ct);
    Task<SesionMesa> UpdateAsync(SesionMesa sesion, CancellationToken ct);
    Task<DateTime?> GetUltimaActividadAsync(int sesionId, CancellationToken ct);
    Task<SesionMesa?> GetActivaConActividadRecienteAsync(int mesaId, int minutosTimeout, CancellationToken ct);
}
