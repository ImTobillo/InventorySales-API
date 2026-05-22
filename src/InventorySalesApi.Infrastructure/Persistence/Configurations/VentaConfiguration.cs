using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.ValueObjects;

namespace Infrastructure.Persistence.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("Ventas");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Fecha)
            .IsRequired();

        // Mapear el Value Object 'Total' (Money) usando un Value Converter
        builder.Property(v => v.Total)
            .HasConversion(
                total => total.Amount,
                decimalValue => new Money(decimalValue))
            .HasColumnName("Total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Convierte el Enum 'EstadoVenta' a string en la base de datos
        builder.Property(v => v.Estado)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        // Relación: Cliente -> Venta (Restrict delete)
        builder.HasOne(v => v.Cliente)
            .WithMany(c => c.Ventas)
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Auditoría (BaseEntity)
        builder.Property(v => v.FechaCreacion)
            .IsRequired();

        builder.Property(v => v.CreadoPor)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.FechaModificacion);

        builder.Property(v => v.ModificadoPor)
            .HasMaxLength(100);
    }
}
