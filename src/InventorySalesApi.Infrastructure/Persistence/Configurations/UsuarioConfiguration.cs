using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.ValueObjects;

namespace Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.NombreUsuario)
            .HasMaxLength(50)
            .IsRequired();

        // Mapear el Value Object 'Email' usando un Value Converter
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                stringValue => new Email(stringValue))
            .HasColumnName("Email")
            .HasMaxLength(255)
            .IsRequired();

        // Configuración del índice único para el Email de Usuario
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(255)
            .IsRequired();

        // Convierte el Enum 'RolUsuario' a string
        builder.Property(u => u.Rol)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(u => u.Activo)
            .IsRequired();

        // Auditoría (BaseEntity)
        builder.Property(u => u.FechaCreacion)
            .IsRequired();

        builder.Property(u => u.CreadoPor)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.FechaModificacion);

        builder.Property(u => u.ModificadoPor)
            .HasMaxLength(100);
    }
}
