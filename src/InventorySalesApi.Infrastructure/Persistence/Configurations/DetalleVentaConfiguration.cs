using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.ValueObjects;

namespace Infrastructure.Persistence.Configurations;

public class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> builder)
    {
        builder.ToTable("DetallesVenta");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Cantidad)
            .IsRequired();

        // Mapear el Value Object 'PrecioUnitario' (Money) usando un Value Converter
        builder.Property(d => d.PrecioUnitario)
            .HasConversion(
                precio => precio.Amount,
                decimalValue => new Money(decimalValue))
            .HasColumnName("PrecioUnitario")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Mapear el Value Object 'Descuento' (Money) usando un Value Converter
        builder.Property(d => d.Descuento)
            .HasConversion(
                descuento => descuento.Amount,
                decimalValue => new Money(decimalValue))
            .HasColumnName("Descuento")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Mapear el Value Object 'Subtotal' (Money) usando un Value Converter
        builder.Property(d => d.Subtotal)
            .HasConversion(
                subtotal => subtotal.Amount,
                decimalValue => new Money(decimalValue))
            .HasColumnName("Subtotal")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Relación: Venta -> DetalleVenta (Cascade delete)
        builder.HasOne(d => d.Venta)
            .WithMany(v => v.Detalles)
            .HasForeignKey(d => d.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación: Producto -> DetalleVenta (Restrict delete)
        builder.HasOne(d => d.Producto)
            .WithMany(p => p.DetallesVenta)
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Auditoría (BaseEntity)
        builder.Property(d => d.FechaCreacion)
            .IsRequired();

        builder.Property(d => d.CreadoPor)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.FechaModificacion);

        builder.Property(d => d.ModificadoPor)
            .HasMaxLength(100);
    }
}
