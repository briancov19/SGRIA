namespace SGRIA.Domain.Entities;

public class Mesa
{
    public int Id { get; set; }
    public int RestauranteId { get; set; }

    public int Numero { get; set; } = default!;
    public int CantidadSillas { get; set; }

    public string QrToken { get; set; } = default!;
    public bool Activa { get; set; } = true;

    public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;

    public Restaurante Restaurante { get; set; } = default!;
    public ICollection<SesionMesa> Sesiones { get; set; } = new List<SesionMesa>();
}
