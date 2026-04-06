using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

internal static class EntityTypeBuilderExtensions
{
    public static void ConfigureAuditColumns<TEntity>(this EntityTypeBuilder<TEntity> builder, bool includeBranchId = true)
        where TEntity : AuditableEntity
    {
        builder.Property(entity => entity.CompanyId).HasDefaultValue(1);
        builder.Property(entity => entity.SiteId).HasDefaultValue(1);

        if (includeBranchId)
        {
            builder.Property(entity => entity.BranchId).HasDefaultValue(1);
        }
        else
        {
            builder.Ignore(entity => entity.BranchId);
        }

        builder.Property(entity => entity.DepartmentId);
        builder.Property(entity => entity.Tag).HasMaxLength(64);
        builder.Property(entity => entity.Comments).HasMaxLength(512);
        builder.Property(entity => entity.DisplayOnWeb).HasDefaultValue(true);
        builder.Property(entity => entity.IsPublished).HasDefaultValue(true);
        builder.Property(entity => entity.DatePublished);
        builder.Property(entity => entity.PublishedBy).HasMaxLength(128);
        builder.Property(entity => entity.SortOrder).HasDefaultValue(0);
        builder.Property(entity => entity.IPAddress).HasMaxLength(64).HasDefaultValue("127.0.0.1");
        builder.Property(entity => entity.CreatedBy).HasMaxLength(128).HasDefaultValue("System");
        builder.Property(entity => entity.DateCreated).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(128);
        builder.Property(entity => entity.LastUpdated);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(128);
        builder.Property(entity => entity.DateDeleted);
        builder.Property(entity => entity.IsDeleted).HasDefaultValue(false);
    }
}
