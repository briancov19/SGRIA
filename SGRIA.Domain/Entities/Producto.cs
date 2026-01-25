namespace SGRIA.Domain.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = default!;
    public decimal Precio { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
