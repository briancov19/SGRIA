using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SGRIA.Api.Filters;

/// <summary>
/// Valida que el parámetro de ruta "sesPublicToken" tenga formato GUID válido.
/// Devuelve 400 Bad Request si no es válido, evitando consultas innecesarias a la BD.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ValidateSesPublicTokenAttribute : ActionFilterAttribute
{
    private const string RouteKey = "sesPublicToken";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.RouteData.Values.TryGetValue(RouteKey, out var value) || value is not string token)
        {
            base.OnActionExecuting(context);
            return;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            context.Result = new BadRequestObjectResult(new { error = "sesPublicToken es requerido." });
            return;
        }

        if (!Guid.TryParse(token, out _))
        {
            context.Result = new BadRequestObjectResult(new { error = "sesPublicToken debe tener formato GUID válido." });
            return;
        }

        base.OnActionExecuting(context);
    }
}
