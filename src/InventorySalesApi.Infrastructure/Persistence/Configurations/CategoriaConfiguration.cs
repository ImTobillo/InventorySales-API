using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventorySalesApi.Domain.Entities;

namespace Infrastructure.Persistence.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Descripcion)
            .HasMaxLength(500);

        builder.Property(c => c.Activo)
            .IsRequired();

        // Auditoría (BaseEntity)
        builder.Property(c => c.FechaCreacion)
            .IsRequired();

        builder.Property(c => c.CreadoPor)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.FechaModificacion);

        builder.Property(c => c.ModificadoPor)
            .HasMaxLength(100);
    }
}
