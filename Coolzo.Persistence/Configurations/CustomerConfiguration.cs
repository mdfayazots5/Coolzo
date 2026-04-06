using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("tblCustomer");
        builder.HasKey(entity => entity.CustomerId).HasName("PK_tblCustomer_CustomerId");

        builder.Property(entity => entity.CustomerId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.CustomerName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.MobileNumber).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.EmailAddress).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.IsGuestCustomer).HasDefaultValue(false);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasOne(entity => entity.User)
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomer_UserId_tblUser_UserId");

        builder.HasIndex(entity => entity.MobileNumber)
            .HasDatabaseName("IDX_tblCustomer_MobileNumber");
        builder.HasIndex(entity => entity.UserId)
            .IsUnique()
            .HasDatabaseName("UK_tblCustomer_UserId")
            .HasFilter("[UserId] IS NOT NULL");

        builder.ConfigureAuditColumns();
    }
}
