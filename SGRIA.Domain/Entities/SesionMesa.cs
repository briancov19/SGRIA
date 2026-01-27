namespace SGRIA.Domain.Entities;

public class SesionMesa
{
    public int Id { get; set; }
    public int MesaId { get; set; }

    public DateTime FechaHoraInicio { get; set; } = DateTime.UtcNow;
    public DateTime? FechaHoraFin { get; set; }

    public int? CantidadPersonas { get; set; }
    public string? Origen { get; set; } // "QR", "Manual", "Desconocido"

    public Mesa Mesa { get; set; } = default!;
    public ICollection<SenalPedido> SenalesPedido { get; set; } = new List<SenalPedido>();
    public ICollection<VotoTagItemMenu> VotosTag { get; set; } = new List<VotoTagItemMenu>();
    public ICollection<SesionParticipante> Participantes { get; set; } = new List<SesionParticipante>();
}