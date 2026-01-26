namespace SGRIA.Domain.Entities;

public class SenalRating
{
    public int Id { get; set; }
    public int SenalPedidoId { get; set; }

    public short Puntaje { get; set; } // 👍=1, 😐=0, 👎=-1
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;

    public SenalPedido SenalPedido { get; set; } = default!;
}