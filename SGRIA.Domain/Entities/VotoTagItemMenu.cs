namespace SGRIA.Domain.Entities;

public class VotoTagItemMenu
{
    public int Id { get; set; }
    public int SesionMesaId { get; set; }
    public int ItemMenuId { get; set; }
    public int TagRapidoId { get; set; }

    public short Valor { get; set; } = 1; // +1 / -1
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;

    public SesionMesa SesionMesa { get; set; } = default!;
    public ItemMenu ItemMenu { get; set; } = default!;
    public TagRapido TagRapido { get; set; } = default!;
}