using Microsoft.EntityFrameworkCore;
using RagPlatform.Domain.Entities;
using RagPlatform.Domain.Interfaces;

namespace RagPlatform.Infrastructure.Persistence;

/// <summary>
/// EF Core context for document metadata / RBAC / audit (the "system of record" that sits
/// alongside Azure AI Search, which holds the actual chunk text + vectors for retrieval).
/// </summary>
public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
    public DbSet<DocumentPermission> DocumentPermissions => Set<DocumentPermission>();
    public DbSet<ProcessingHistoryEntry> ProcessingHistory => Set<ProcessingHistoryEntry>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}
