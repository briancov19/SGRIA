using SGRIA.Domain.Entities;

namespace SGRIA.Application.Services;

/// <summary>
/// Servicio para calcular el score de confianza de un pedido.
/// </summary>
public class ConfianzaService
{
    /// <summary>
    /// Calcula el score de confianza (0.0 a 1.0) para un pedido basado en heurísticas.
    /// </summary>
    public decimal CalcularConfianza(
        SesionParticipante? participante,
        SesionMesa sesion,
        int totalPedidosEnSesion,
        DateTime ahora)
    {
        decimal confianza = 0.5m; // Base: 50% de confianza

        // 1. Sesión "fresh": si el participante se unió por QR en los últimos 5 min → + confianza
        if (participante != null)
        {
            var minutosDesdeJoin = (ahora - participante.FechaHoraJoin).TotalMinutes;
            if (minutosDesdeJoin <= 5)
            {
                confianza += 0.2m; // +20% si se unió recientemente
            }
            else if (minutosDesdeJoin > 60)
            {
                confianza -= 0.1m; // -10% si lleva más de 1 hora
            }

            // Actividad reciente del participante
            var minutosDesdeActividad = (ahora - participante.UltimaActividad).TotalMinutes;
            if (minutosDesdeActividad <= 10)
            {
                confianza += 0.1m; // +10% si tiene actividad reciente
            }
        }

        // 2. Actividad humana razonable: si hay muchos pedidos en poco tiempo → baja confianza
        var minutosDesdeInicioSesion = (ahora - sesion.FechaHoraInicio).TotalMinutes;
        if (minutosDesdeInicioSesion > 0)
        {
            var pedidosPorMinuto = totalPedidosEnSesion / minutosDesdeInicioSesion;
            if (pedidosPorMinuto > 10) // Más de 10 pedidos por minuto es sospechoso
            {
                confianza -= 0.3m; // -30% si hay actividad anormal
            }
            else if (pedidosPorMinuto > 5)
            {
                confianza -= 0.1m; // -10% si hay actividad alta
            }
        }

        // 3. "Distancia" de sesión: si la sesión fue creada hace mucho y sigue abierta → bajar confianza
        var horasDesdeInicio = (ahora - sesion.FechaHoraInicio).TotalHours;
        if (horasDesdeInicio > 24)
        {
            confianza -= 0.2m; // -20% si la sesión tiene más de 24 horas
        }
        else if (horasDesdeInicio > 12)
        {
            confianza -= 0.1m; // -10% si tiene más de 12 horas
        }

        // 4. Si la sesión está cerrada, confianza mínima
        if (sesion.FechaHoraFin.HasValue)
        {
            confianza = 0.1m; // Muy baja confianza si la sesión está cerrada
        }

        // Asegurar que esté entre 0.0 y 1.0
        return Math.Max(0.0m, Math.Min(1.0m, confianza));
    }
}
