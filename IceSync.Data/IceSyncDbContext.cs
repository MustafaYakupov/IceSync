using IceSync.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IceSync.Data;

public class IceSyncDbContext : DbContext
{
    public IceSyncDbContext(DbContextOptions<IceSyncDbContext> options) : base(options) { }

    public DbSet<Workflow> Workflows => Set<Workflow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Workflow>()
            .HasIndex(x => x.ApiWorkflowId)
            .IsUnique();

        modelBuilder.Entity<Workflow>()
            .HasQueryFilter(x => !x.IsDeleted); // hide deleted by default
    }
}
