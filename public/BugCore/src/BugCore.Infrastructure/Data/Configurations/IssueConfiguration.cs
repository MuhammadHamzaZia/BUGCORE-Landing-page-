using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BugCore.Core.Entities;

namespace BugCore.Infrastructure.Data.Configurations;

public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.ToTable("BugCoreIssues");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Summary).IsRequired().HasMaxLength(250);
        builder.Property(i => i.Description).IsRequired();

        builder.HasOne(i => i.Project)
            .WithMany(p => p.Issues)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Reporter)
            .WithMany(u => u.ReportedIssues)
            .HasForeignKey(i => i.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Handler)
            .WithMany(u => u.AssignedIssues)
            .HasForeignKey(i => i.HandlerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.Category)
            .WithMany(c => c.Issues)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
