namespace SGRIA.Domain.Entities;

public class SesionMesa
{
    public int Id { get; set; }
    public int MesaId { get; set; }

    /// <summary>
    /// Token público único (GUID) para identificar la sesión en APIs públicas.
    /// Evita enumeración de sesionId interno.
    /// </summary>
    public string SesPublicToken { get; set; } = default!;

    public DateTime FechaHoraInicio { get; set; } = DateTime.UtcNow;
    public DateTime? FechaHoraFin { get; set; }

    /// <summary>
    /// Última actividad registrada en la sesión (UTC).
    /// Se actualiza en cada acción del cliente (pedido, rating, etc.).
    /// </summary>
    public DateTime FechaHoraUltActividad { get; set; } = DateTime.UtcNow;

    public int? CantidadPersonas { get; set; }
    public string? Origen { get; set; } // "QR", "Manual", "Desconocido"

    public Mesa Mesa { get; set; } = default!;
    public ICollection<SenalPedido> SenalesPedido { get; set; } = new List<SenalPedido>();
    public ICollection<VotoTagItemMenu> VotosTag { get; set; } = new List<VotoTagItemMenu>();
    public ICollection<SesionParticipante> Participantes { get; set; } = new List<SesionParticipante>();
}