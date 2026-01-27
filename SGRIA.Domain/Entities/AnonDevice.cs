namespace SGRIA.Domain.Entities;

/// <summary>
/// Representa un dispositivo anónimo identificado por un hash único.
/// No almacena información sensible, solo un hash calculado del clientId + salt del servidor.
/// </summary>
public class AnonDevice
{
    public int Id { get; set; }
    
    /// <summary>
    /// Hash SHA256 único del dispositivo (clientId + serverSalt)
    /// </summary>
    public string DeviceHash { get; set; } = default!;
    
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    public ICollection<SesionParticipante> Participantes { get; set; } = new List<SesionParticipante>();
}
