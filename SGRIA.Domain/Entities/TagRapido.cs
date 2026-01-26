namespace SGRIA.Domain.Entities;

public class TagRapido
{
    public int Id { get; set; }
    public string Nombre { get; set; } = default!;
    public string? Tipo { get; set; } // "Sabor", "Porcion", "Advertencia", etc.
    public bool Activo { get; set; } = true;

    public ICollection<VotoTagItemMenu> Votos { get; set; } = new List<VotoTagItemMenu>();
}