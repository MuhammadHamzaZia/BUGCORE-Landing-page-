using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BugCore.Core.Entities;

namespace BugCore.Infrastructure.Data.Configurations;

public class ProjectVersionConfiguration : IEntityTypeConfiguration<ProjectVersion>
{
    public void Configure(EntityTypeBuilder<ProjectVersion> builder)
    {
        builder.ToTable("BugCoreProjectVersions");
        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.Version)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(pv => pv.Description)
            .HasMaxLength(500);

        // In BugCoreBT, version is unique per project (idx_project_version)
        builder.HasIndex(pv => new { pv.ProjectId, pv.Version }).IsUnique();

        builder.HasOne(pv => pv.Project)
            .WithMany(p => p.Versions)
            .HasForeignKey(pv => pv.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
