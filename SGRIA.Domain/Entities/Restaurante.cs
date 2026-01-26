namespace SGRIA.Domain.Entities;

public class Restaurante
{
public int Id { get; set; }
public string Nombre { get; set; } = default!;
public string TimeZone { get; set; } = "America/Montevideo";
public bool Activo { get; set; } = true;
public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;


public ICollection<Mesa> Mesas { get; set; } = new List<Mesa>();
public ICollection<ItemMenu> ItemsMenu { get; set; } = new List<ItemMenu>();
}