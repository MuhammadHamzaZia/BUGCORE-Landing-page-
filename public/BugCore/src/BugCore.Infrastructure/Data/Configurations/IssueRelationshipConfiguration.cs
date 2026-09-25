using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BugCore.Core.Entities;

namespace BugCore.Infrastructure.Data.Configurations;

public class IssueRelationshipConfiguration : IEntityTypeConfiguration<IssueRelationship>
{
    public void Configure(EntityTypeBuilder<IssueRelationship> builder)
    {
        builder.ToTable("BugCoreIssueRelationships");
        builder.HasKey(ir => ir.Id);

        builder.HasOne(ir => ir.SourceIssue)
            .WithMany(i => i.SourceRelationships)
            .HasForeignKey(ir => ir.SourceIssueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ir => ir.DestinationIssue)
            .WithMany(i => i.DestinationRelationships)
            .HasForeignKey(ir => ir.DestinationIssueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
