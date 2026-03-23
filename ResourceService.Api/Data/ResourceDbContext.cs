using Microsoft.EntityFrameworkCore;
using ResourceService.Api.Domain;

namespace ResourceService.Api.Data;

public sealed class ResourceDbContext : DbContext
{
    public ResourceDbContext(DbContextOptions<ResourceDbContext> options) : base(options)
    {
    }

    public DbSet<Resource> Resources => Set<Resource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var resource = modelBuilder.Entity<Resource>();

        resource.ToTable("resources");
        resource.HasKey(x => x.Id);

        resource.Property(x => x.Name).HasMaxLength(200).IsRequired();
        resource.Property(x => x.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
        resource.Property(x => x.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        resource.Property(x => x.OfficeAddress).HasMaxLength(300);
        resource.Property(x => x.Description).HasMaxLength(2000);
        resource.Property(x => x.AllowedRoles)
            .HasColumnType("text[]")
            .IsRequired();

        resource.HasIndex(x => new { x.OrganizationId, x.Status });
        resource.HasIndex(x => new { x.OrganizationId, x.Type });
    }
}
