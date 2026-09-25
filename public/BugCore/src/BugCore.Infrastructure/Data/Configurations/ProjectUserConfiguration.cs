using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BugCore.Core.Entities;

namespace BugCore.Infrastructure.Data.Configurations;

public class ProjectUserConfiguration : IEntityTypeConfiguration<ProjectUser>
{
    public void Configure(EntityTypeBuilder<ProjectUser> builder)
    {
        builder.ToTable("BugCoreProjectUsers");
        builder.HasKey(pu => new { pu.ProjectId, pu.UserId });

        builder.HasOne(pu => pu.Project)
            .WithMany(p => p.UserAssignments)
            .HasForeignKey(pu => pu.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pu => pu.User)
            .WithMany(u => u.ProjectAssignments)
            .HasForeignKey(pu => pu.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
