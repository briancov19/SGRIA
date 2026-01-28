using Microsoft.EntityFrameworkCore;
using SGRIA.Domain.Entities;

namespace SGRIA.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<NotificacionCliente> NotificacionesClientes { get; set; }
    public DbSet<Restaurante> Restaurantes => Set<Restaurante>();
    public DbSet<SesionMesa> SesionesMesa => Set<SesionMesa>();
    public DbSet<ItemMenu> ItemsMenu => Set<ItemMenu>();
    public DbSet<ItemMenuAlias> ItemsMenuAlias => Set<ItemMenuAlias>();
    public DbSet<SenalPedido> SenalesPedido => Set<SenalPedido>();
    public DbSet<SenalRating> SenalesRating => Set<SenalRating>();
    public DbSet<TagRapido> TagsRapido => Set<TagRapido>();
    public DbSet<VotoTagItemMenu> VotosTagItemMenu => Set<VotoTagItemMenu>();
    public DbSet<AnonDevice> AnonDevices => Set<AnonDevice>();
    public DbSet<SesionParticipante> SesionParticipantes => Set<SesionParticipante>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // NotificacionCliente (legacy)
        modelBuilder.Entity<NotificacionCliente>(builder =>
        {
            builder.ToTable("notificaciones_cliente");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("NclId");
            builder.Property(x => x.FechaCreacion).HasColumnName("NclFechaCreacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.Atendida).HasColumnName("NclAtendida");
            builder.Property(x => x.MesaId).HasColumnName("MesId");
            builder.HasOne(x => x.Mesa)
                .WithMany()
                .HasForeignKey(x => x.MesaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Restaurante
        modelBuilder.Entity<Restaurante>(builder =>
        {
            builder.ToTable("restaurantes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("ResId");
            builder.Property(x => x.Nombre).HasColumnName("ResNombre").HasMaxLength(200).IsRequired();
            builder.Property(x => x.TimeZone).HasColumnName("ResTimeZone").HasMaxLength(50).HasDefaultValue("America/Montevideo");
            builder.Property(x => x.Activo).HasColumnName("ResActivo").HasDefaultValue(true);
            builder.Property(x => x.FechaCreacion).HasColumnName("ResFchaCreacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            builder.HasIndex(x => x.Nombre);
        });

        // Mesa
        modelBuilder.Entity<Mesa>(builder =>
        {
            builder.ToTable("mesas");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("MesId");
            builder.Property(x => x.RestauranteId).HasColumnName("MesResId");
            builder.Property(x => x.Numero).HasColumnName("MesNumero").IsRequired();
            builder.Property(x => x.CantidadSillas).HasColumnName("MesCantidadSillas").HasDefaultValue(4);
            builder.Property(x => x.QrToken).HasColumnName("MesQrToken").HasMaxLength(100).IsRequired();
            builder.Property(x => x.Activa).HasColumnName("MesActiva").HasDefaultValue(true);
            builder.Property(x => x.FechaModificacion).HasColumnName("MesFchaModificacion").HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasIndex(x => x.QrToken).IsUnique();
            builder.HasIndex(x => new { x.RestauranteId, x.Numero });
            
            builder.HasOne(x => x.Restaurante)
                .WithMany(r => r.Mesas)
                .HasForeignKey(x => x.RestauranteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SesionMesa
        modelBuilder.Entity<SesionMesa>(builder =>
        {
            builder.ToTable("sesiones_mesa");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("SesId");
            builder.Property(x => x.MesaId).HasColumnName("SesMesId");
            builder.Property(x => x.SesPublicToken).HasColumnName("SesPublicToken").HasMaxLength(36).IsRequired();
            builder.Property(x => x.FechaHoraInicio).HasColumnName("SesFchaHoraInicio").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.FechaHoraFin).HasColumnName("SesFchaHoraFin");
            builder.Property(x => x.FechaHoraUltActividad).HasColumnName("SesFchaHoraUltActividad").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.CantidadPersonas).HasColumnName("SesCantidadPersonas");
            builder.Property(x => x.Origen).HasColumnName("SesOrigen").HasMaxLength(20).HasDefaultValue("QR");

            builder.HasIndex(x => x.MesaId);
            builder.HasIndex(x => x.FechaHoraInicio);
            builder.HasIndex(x => new { x.MesaId, x.FechaHoraFin });
            builder.HasIndex(x => x.SesPublicToken).IsUnique();
            builder.HasIndex(x => x.FechaHoraUltActividad);
            
            builder.HasOne(x => x.Mesa)
                .WithMany(m => m.Sesiones)
                .HasForeignKey(x => x.MesaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ItemMenu
        modelBuilder.Entity<ItemMenu>(builder =>
        {
            builder.ToTable("items_menu");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("ItmId");
            builder.Property(x => x.RestauranteId).HasColumnName("ItmResId");
            builder.Property(x => x.Nombre).HasColumnName("ItmNombre").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Descripcion).HasColumnName("ItmDescripcion").HasMaxLength(1000);
            builder.Property(x => x.Categoria).HasColumnName("ItmCategoria").HasMaxLength(100);
            builder.Property(x => x.Precio).HasColumnName("ItmPrecio").HasPrecision(18, 2);
            builder.Property(x => x.Activo).HasColumnName("ItmActivo").HasDefaultValue(true);
            builder.Property(x => x.ImagenUrl).HasColumnName("ItmImagenUrl").HasMaxLength(500);

            builder.HasIndex(x => x.RestauranteId);
            builder.HasIndex(x => new { x.RestauranteId, x.Activo });
            builder.HasIndex(x => x.Categoria);
            
            builder.HasOne(x => x.Restaurante)
                .WithMany(r => r.ItemsMenu)
                .HasForeignKey(x => x.RestauranteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ItemMenuAlias
        modelBuilder.Entity<ItemMenuAlias>(builder =>
        {
            builder.ToTable("items_menu_alias");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("AliId");
            builder.Property(x => x.ItemMenuId).HasColumnName("AliItmId");
            builder.Property(x => x.AliasTexto).HasColumnName("AliTexto").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Activo).HasColumnName("AliActivo").HasDefaultValue(true);

            builder.HasIndex(x => x.ItemMenuId);
            builder.HasIndex(x => new { x.AliasTexto, x.Activo });
            
            builder.HasOne(x => x.ItemMenu)
                .WithMany(i => i.Aliases)
                .HasForeignKey(x => x.ItemMenuId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SenalPedido
        modelBuilder.Entity<SenalPedido>(builder =>
        {
            builder.ToTable("senales_pedido");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("PedId");
            builder.Property(x => x.SesionMesaId).HasColumnName("PedSesId");
            builder.Property(x => x.ItemMenuId).HasColumnName("PedItmId");
            builder.Property(x => x.Cantidad).HasColumnName("PedCantidad").HasDefaultValue(1);
            builder.Property(x => x.FechaHoraConfirmacion).HasColumnName("PedFchaHoraConfirmacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.IngresadoPor).HasColumnName("PedIngresadoPor").HasMaxLength(20).HasDefaultValue("Cliente");
            builder.Property(x => x.Confianza).HasColumnName("PedConfianza").HasPrecision(3, 2);

            builder.HasIndex(x => x.SesionMesaId);
            builder.HasIndex(x => x.ItemMenuId);
            builder.HasIndex(x => x.FechaHoraConfirmacion);
            builder.HasIndex(x => new { x.ItemMenuId, x.FechaHoraConfirmacion });
            
            builder.HasOne(x => x.SesionMesa)
                .WithMany(s => s.SenalesPedido)
                .HasForeignKey(x => x.SesionMesaId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(x => x.ItemMenu)
                .WithMany(i => i.SenalesPedido)
                .HasForeignKey(x => x.ItemMenuId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SenalRating
        modelBuilder.Entity<SenalRating>(builder =>
        {
            builder.ToTable("senales_rating");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("RatId");
            builder.Property(x => x.SenalPedidoId).HasColumnName("RatPedId");
            builder.Property(x => x.Puntaje).HasColumnName("RatPuntaje").IsRequired(); // -1, 0, 1
            builder.Property(x => x.FechaHora).HasColumnName("RatFchaHora").HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasIndex(x => x.SenalPedidoId).IsUnique();
            builder.HasIndex(x => x.FechaHora); // Para consultas por fecha
            builder.HasIndex(x => new { x.Puntaje, x.FechaHora });
            
            builder.HasOne(x => x.SenalPedido)
                .WithOne(p => p.Rating)
                .HasForeignKey<SenalRating>(x => x.SenalPedidoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TagRapido
        modelBuilder.Entity<TagRapido>(builder =>
        {
            builder.ToTable("tags_rapido");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("TagId");
            builder.Property(x => x.Nombre).HasColumnName("TagNombre").HasMaxLength(100).IsRequired();
            builder.Property(x => x.Tipo).HasColumnName("TagTipo").HasMaxLength(50);
            builder.Property(x => x.Activo).HasColumnName("TagActivo").HasDefaultValue(true);

            builder.HasIndex(x => new { x.Nombre, x.Activo });
        });

        // VotoTagItemMenu
        modelBuilder.Entity<VotoTagItemMenu>(builder =>
        {
            builder.ToTable("votos_tag_item_menu");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("VtiId");
            builder.Property(x => x.SesionMesaId).HasColumnName("VtiSesId");
            builder.Property(x => x.ItemMenuId).HasColumnName("VtiItmId");
            builder.Property(x => x.TagRapidoId).HasColumnName("VtiTagId");
            builder.Property(x => x.Valor).HasColumnName("VtiValor").HasDefaultValue((short)1); // +1 / -1
            builder.Property(x => x.FechaHora).HasColumnName("VtiFchaHora").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Índice único para evitar duplicados (sesion+item+tag)
            builder.HasIndex(x => new { x.SesionMesaId, x.ItemMenuId, x.TagRapidoId })
                .IsUnique();
            builder.HasIndex(x => x.ItemMenuId);
            
            builder.HasOne(x => x.SesionMesa)
                .WithMany(s => s.VotosTag)
                .HasForeignKey(x => x.SesionMesaId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(x => x.ItemMenu)
                .WithMany(i => i.VotosTag)
                .HasForeignKey(x => x.ItemMenuId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(x => x.TagRapido)
                .WithMany(t => t.Votos)
                .HasForeignKey(x => x.TagRapidoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AnonDevice
        modelBuilder.Entity<AnonDevice>(builder =>
        {
            builder.ToTable("anon_devices");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("DevId");
            builder.Property(x => x.DeviceHash).HasColumnName("DevHash").HasMaxLength(64).IsRequired();
            builder.Property(x => x.FechaCreacion).HasColumnName("DevFchaCreacion").HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasIndex(x => x.DeviceHash).IsUnique();
        });

        // SesionParticipante
        modelBuilder.Entity<SesionParticipante>(builder =>
        {
            builder.ToTable("sesion_participantes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("ParId");
            builder.Property(x => x.SesionMesaId).HasColumnName("ParSesId");
            builder.Property(x => x.AnonDeviceId).HasColumnName("ParDevId");
            builder.Property(x => x.FechaHoraJoin).HasColumnName("ParFchaHoraJoin").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UltimaActividad).HasColumnName("ParUltimaActividad").HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasIndex(x => new { x.SesionMesaId, x.AnonDeviceId });
            builder.HasIndex(x => x.UltimaActividad);
            
            builder.HasOne(x => x.SesionMesa)
                .WithMany(s => s.Participantes)
                .HasForeignKey(x => x.SesionMesaId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(x => x.AnonDevice)
                .WithMany(d => d.Participantes)
                .HasForeignKey(x => x.AnonDeviceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
