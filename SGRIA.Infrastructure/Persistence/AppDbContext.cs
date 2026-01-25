using Microsoft.EntityFrameworkCore;
using SGRIA.Domain.Entities;

namespace SGRIA.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<NotificacionCliente> NotificacionesClientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Indicar a Postgres que use UTC para todas las fechas
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("productos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Precio).HasPrecision(18, 2);
            e.Property(x => x.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Mesa>(builder =>
        {
            builder.ToTable("mesas");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("MesId");
            builder.Property(x => x.Numero).HasColumnName("MesNumero");
            builder.Property(x => x.CantidadSillas).HasColumnName("MesCantidadSillas");
            builder.Property(x => x.FechaModificacion).HasColumnName("MesFchaModificacion");
        });

        modelBuilder.Entity<NotificacionCliente>(builder =>
        {
            builder.ToTable("notificaciones_cliente");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("NclId");
            builder.Property(x => x.FechaCreacion).HasColumnName("NclFechaCreacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.Atendida).HasColumnName("NclAtendida");
            builder.Property(x => x.MesaId).HasColumnName("MesId"); // FK hacia Mesas

            // Relación Unidireccional: Notificación -> Mesa
            builder.HasOne(x => x.Mesa)
                .WithMany() 
                .HasForeignKey(x => x.MesaId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
