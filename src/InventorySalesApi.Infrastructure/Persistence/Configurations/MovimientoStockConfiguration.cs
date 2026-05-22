using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventorySalesApi.Domain.Entities;

namespace Infrastructure.Persistence.Configurations;

public class MovimientoStockConfiguration : IEntityTypeConfiguration<MovimientoStock>
{
    public void Configure(EntityTypeBuilder<MovimientoStock> builder)
    {
        builder.ToTable("MovimientosStock");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Cantidad)
            .IsRequired();

        // Convierte el Enum 'TipoMovimiento' a string
        builder.Property(m => m.Tipo)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(m => m.Fecha)
            .IsRequired();

        builder.Property(m => m.Descripcion)
            .HasMaxLength(255)
            .IsRequired();

        // Relación: Producto -> MovimientosStock (Cascade delete)
        builder.HasOne(m => m.Producto)
            .WithMany(p => p.MovimientosStock)
            .HasForeignKey(m => m.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación: Usuario -> MovimientosStock (Restrict delete)
        builder.HasOne(m => m.Usuario)
            .WithMany(u => u.MovimientosStock)
            .HasForeignKey(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Auditoría (BaseEntity)
        builder.Property(m => m.FechaCreacion)
            .IsRequired();

        builder.Property(m => m.CreadoPor)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.FechaModificacion);

        builder.Property(m => m.ModificadoPor)
            .HasMaxLength(100);
    }
}
