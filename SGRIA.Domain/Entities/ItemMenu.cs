namespace SGRIA.Domain.Entities;

public class ItemMenu
{
    public int Id { get; set; }
    public int RestauranteId { get; set; }


    public string Nombre { get; set; } = default!;
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }
    public decimal? Precio { get; set; }


    public bool Activo { get; set; } = true;
    public string? ImagenUrl { get; set; }


    public Restaurante Restaurante { get; set; } = default!;
    public ICollection<ItemMenuAlias> Aliases { get; set; } = new List<ItemMenuAlias>();
    public ICollection<SenalPedido> SenalesPedido { get; set; } = new List<SenalPedido>();
    public ICollection<VotoTagItemMenu> VotosTag { get; set; } = new List<VotoTagItemMenu>();
}