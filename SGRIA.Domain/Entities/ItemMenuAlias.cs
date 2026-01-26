namespace SGRIA.Domain.Entities;

public class ItemMenuAlias
{
    public int Id { get; set; }
    public int ItemMenuId { get; set; }

    public string AliasTexto { get; set; } = default!;
    public bool Activo { get; set; } = true;

    public ItemMenu ItemMenu { get; set; } = default!;
}