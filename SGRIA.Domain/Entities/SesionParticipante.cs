namespace SGRIA.Domain.Entities;

/// <summary>
/// Relación entre una sesión de mesa y un dispositivo anónimo.
/// Permite rastrear la actividad de cada participante sin almacenar PII.
/// </summary>
public class SesionParticipante
{
    public int Id { get; set; }
    
    public int SesionMesaId { get; set; }
    public int AnonDeviceId { get; set; }
    
    /// <summary>
    /// Fecha/hora en que el dispositivo se unió a la sesión
    /// </summary>
    public DateTime FechaHoraJoin { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Última actividad registrada del participante en esta sesión
    /// </summary>
    public DateTime UltimaActividad { get; set; } = DateTime.UtcNow;
    
    public SesionMesa SesionMesa { get; set; } = default!;
    public AnonDevice AnonDevice { get; set; } = default!;
}
