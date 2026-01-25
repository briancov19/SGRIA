namespace SGRIA.Domain.Entities;

public class NotificacionCliente
{
    public int Id { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public bool Atendida { get; set; } = false;

    public int MesaId { get; set; }
    public Mesa Mesa { get; set; } = default!; 

    public bool EsValida(int minutosMaximos) 
    {
        return !Atendida && (DateTime.UtcNow - FechaCreacion).TotalMinutes < minutosMaximos;
    }
}