namespace SGRIA.Domain.Entities;

public class Mesa
{
    public int Id { get; set; }
    public int Numero { get; set; } = default!;
    public int CantidadSillas { get; set; }
    public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;
}
