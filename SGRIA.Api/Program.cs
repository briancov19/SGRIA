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

// Services
builder.Services.AddScoped<MesaService>();
builder.Services.AddScoped<NotificacionClienteService>();
builder.Services.AddScoped<SesionMesaService>();
builder.Services.AddScoped<SenalPedidoService>();
builder.Services.AddScoped<SenalRatingService>();
builder.Services.AddScoped<EstadisticasService>();
builder.Services.AddScoped<MesaQrService>();
builder.Services.AddScoped<RestauranteService>();
builder.Services.AddScoped<ItemMenuService>();
builder.Services.AddScoped<ItemMenuAliasService>();
builder.Services.AddScoped<TagRapidoService>();
builder.Services.AddScoped<FeedService>();
builder.Services.AddScoped<ItemSocialService>();
builder.Services.AddScoped<TagVotoService>();
builder.Services.AddScoped<AnonDeviceService>();
builder.Services.AddScoped<ConfianzaService>();
builder.Services.AddScoped<RateLimitService>();

// 1) DbContext (Postgres)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) Repositories
builder.Services.AddScoped<IMesaRepository, EfMesaRepository>();
builder.Services.AddScoped<INotificacionClienteRepository, EfNotificacionClienteRepository>();
builder.Services.AddScoped<ISesionMesaRepository, EfSesionMesaRepository>();
builder.Services.AddScoped<ISenalPedidoRepository, EfSenalPedidoRepository>();
builder.Services.AddScoped<ISenalRatingRepository, EfSenalRatingRepository>();
builder.Services.AddScoped<IItemMenuRepository, EfItemMenuRepository>();
builder.Services.AddScoped<IEstadisticasRepository, EfEstadisticasRepository>();
builder.Services.AddScoped<IRestauranteRepository, EfRestauranteRepository>();
builder.Services.AddScoped<IItemMenuAliasRepository, EfItemMenuAliasRepository>();
builder.Services.AddScoped<ITagRapidoRepository, EfTagRapidoRepository>();
builder.Services.AddScoped<IItemSocialRepository, EfItemSocialRepository>();
builder.Services.AddScoped<ITagVotoRepository, EfTagVotoRepository>();
builder.Services.AddScoped<IAnonDeviceRepository, EfAnonDeviceRepository>();
builder.Services.AddScoped<ISesionParticipanteRepository, EfSesionParticipanteRepository>();

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
