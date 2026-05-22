using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.ValueObjects;

namespace Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Apellido)
            .HasMaxLength(100)
            .IsRequired();

        // Mapear el Value Object 'Email' usando un Value Converter
        builder.Property(c => c.Email)
            .HasConversion(
                email => email.Value,
                stringValue => new Email(stringValue))
            .HasColumnName("Email")
            .HasMaxLength(255)
            .IsRequired();
            
        // Índice único para evitar emails duplicados
        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.Property(c => c.Telefono)
            .HasMaxLength(30);

        builder.Property(c => c.DocumentoIdentidad)
            .HasMaxLength(30)
            .IsRequired();

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
