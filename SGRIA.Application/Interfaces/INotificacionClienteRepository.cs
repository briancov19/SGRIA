using SGRIA.Domain.Entities;
namespace SGRIA.Application.Interfaces;

public interface INotificacionClienteRepository
{
    // Obtener una notificación específica por su ID 
    Task<NotificacionCliente?> GetByIdAsync(int id, CancellationToken ct);

    // Obtener las notificaciones que no han sido atendidas y están dentro del tiempo de validez (Para la TV)
    Task<List<NotificacionCliente>> GetActivasAsync(int minutosCorte, CancellationToken ct);

    // Crear la notificación (cuando el cliente presiona el botón)
    Task<NotificacionCliente> AddAsync(NotificacionCliente notificacion, CancellationToken ct);

    // Método para que el moso la marque como atendida
    // Devuelve bool para confirmar si se encontró y actualizó correctamente
    Task<bool> MarcarComoAtendidaAsync(int id, CancellationToken ct);
}
