namespace SGRIA.Domain.Entities;

public class SenalPedido
{
    public int Id { get; set; }
    public int SesionMesaId { get; set; }
    public int ItemMenuId { get; set; }

    public int Cantidad { get; set; } = 1;
    public DateTime FechaHoraConfirmacion { get; set; } = DateTime.UtcNow;

    public string IngresadoPor { get; set; } = "Cliente"; // "Cliente", "Mozo", "Sistema"
    public decimal? Confianza { get; set; } // 0..1 (para futuro)

    public SesionMesa SesionMesa { get; set; } = default!;
    public ItemMenu ItemMenu { get; set; } = default!;
    public SenalRating? Rating { get; set; }
}