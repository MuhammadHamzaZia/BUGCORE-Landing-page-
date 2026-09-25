using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BugCore.Core.Entities;

namespace BugCore.Infrastructure.Data.Configurations;

public class IssueMonitorConfiguration : IEntityTypeConfiguration<IssueMonitor>
{
    public void Configure(EntityTypeBuilder<IssueMonitor> builder)
    {
        builder.ToTable("BugCoreIssueMonitors");
        builder.HasKey(im => new { im.IssueId, im.UserId });

        builder.HasOne(im => im.Issue)
            .WithMany(i => i.Monitors)
            .HasForeignKey(im => im.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(im => im.User)
            .WithMany(u => u.MonitoredIssues)
            .HasForeignKey(im => im.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
