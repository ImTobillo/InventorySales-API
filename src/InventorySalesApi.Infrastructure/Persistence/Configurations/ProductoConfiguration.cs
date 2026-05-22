using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.ValueObjects;

namespace Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku)
            .HasMaxLength(50)
            .IsRequired();

        // Configuración de índice único para SKU
        builder.HasIndex(p => p.Sku)
            .IsUnique();

        builder.Property(p => p.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(p => p.Descripcion)
            .HasMaxLength(500);

        // Mapear el Value Object 'Precio' (Money) usando un Value Converter
        builder.Property(p => p.Precio)
            .HasConversion(
                precio => precio.Amount,
                decimalValue => new Money(decimalValue))
            .HasColumnName("Precio")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Stock)
            .IsRequired();

        // Relación: Categoria -> Producto (Restrict delete)
        builder.HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Auditoría (BaseEntity)
        builder.Property(p => p.FechaCreacion)
            .IsRequired();

        builder.Property(p => p.CreadoPor)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.FechaModificacion);

        builder.Property(p => p.ModificadoPor)
            .HasMaxLength(100);
    }
}
