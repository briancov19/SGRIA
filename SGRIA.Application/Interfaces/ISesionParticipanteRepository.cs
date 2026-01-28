using SGRIA.Domain.Entities;

namespace SGRIA.Application.Interfaces;

public interface ISesionParticipanteRepository
{
    Task<SesionParticipante?> GetBySesionAndDeviceAsync(int sesionId, int anonDeviceId, CancellationToken ct);
    Task<SesionParticipante> CreateAsync(SesionParticipante participante, CancellationToken ct);
    Task<SesionParticipante> UpdateAsync(SesionParticipante participante, CancellationToken ct);
    
    /// <summary>
    /// Obtiene el participante activo de una sesión por deviceHash
    /// </summary>
    Task<SesionParticipante?> GetActivoBySesionAndDeviceHashAsync(int sesionId, string deviceHash, CancellationToken ct);
    
    /// <summary>
    /// Cuenta pedidos creados por un participante en una sesión en los últimos X minutos
    /// </summary>
    Task<int> CountPedidosByParticipanteEnVentanaAsync(int sesionParticipanteId, int minutos, CancellationToken ct);
    
    /// <summary>
    /// Cuenta ratings actualizados por un participante en una sesión en los últimos X minutos
    /// </summary>
    Task<int> CountRatingsByParticipanteEnVentanaAsync(int sesionParticipanteId, int minutos, CancellationToken ct);

    /// <summary>
    /// Cuenta votos de tag creados/actualizados en la sesión del participante en los últimos X minutos
    /// </summary>
    Task<int> CountTagVotosByParticipanteEnVentanaAsync(int sesionParticipanteId, int minutos, CancellationToken ct);
}
