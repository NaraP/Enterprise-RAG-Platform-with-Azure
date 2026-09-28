using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RagPlatform.Domain.Entities;

namespace RagPlatform.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileName).HasMaxLength(512).IsRequired();
        builder.Property(d => d.StoragePath).HasMaxLength(1024).IsRequired();
        builder.Property(d => d.ContentHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(d => new { d.TenantId, d.ContentHash });
        builder.HasIndex(d => d.OwnerUserId);

        builder.Property(d => d.Metadata)
            .HasConversion(
                m => System.Text.Json.JsonSerializer.Serialize(m, (System.Text.Json.JsonSerializerOptions?)null),
                s => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(s, (System.Text.Json.JsonSerializerOptions?)null) ?? new());

        builder.OwnsMany(d => d.Permissions, nb =>
        {
            nb.ToTable("DocumentPermissions");
            nb.WithOwner().HasForeignKey(p => p.DocumentId);
            nb.HasKey(p => p.Id);
        });

        builder.OwnsMany(d => d.History, nb =>
        {
            nb.ToTable("ProcessingHistory");
            nb.WithOwner().HasForeignKey(h => h.DocumentId);
            nb.HasKey(h => h.Id);
        });
    }
}
