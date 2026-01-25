using Microsoft.EntityFrameworkCore;
using SGRIA.Application.Interfaces;
using SGRIA.Application.Services;
using SGRIA.Infrastructure.Persistence;
using SGRIA.Infrastructure.Repositories;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<MesaService>();
builder.Services.AddScoped<NotificacionClienteService>();

// 1) DbContext (Postgres)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) Repo EF 
builder.Services.AddScoped<IProductoRepository, EfProductoRepository>();
builder.Services.AddScoped<IMesaRepository, EfMesaRepository>();
builder.Services.AddScoped<INotificacionClienteRepository, EfNotificacionClienteRepository>();

var app = builder.Build();

// Aplicar migraciones al iniciar con manejo de errores
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Esto es clave: espera a que la DB esté lista antes de migrar
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al migrar la base de datos. ¿Está el contenedor de Postgres corriendo?");
    }
}

// Swagger fuera del if de Development (Opcional, pero útil si pruebas en Docker)
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SGRIA API v1"));

app.MapControllers();
app.Run();
