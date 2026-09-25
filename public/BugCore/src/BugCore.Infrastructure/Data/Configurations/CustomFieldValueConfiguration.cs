using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BugCore.Core.Entities;

namespace BugCore.Infrastructure.Data.Configurations;

public class CustomFieldValueConfiguration : IEntityTypeConfiguration<CustomFieldValue>
{
    public void Configure(EntityTypeBuilder<CustomFieldValue> builder)
    {
        builder.ToTable("BugCoreCustomFieldValues");
        builder.HasKey(cfv => new { cfv.CustomFieldId, cfv.IssueId });

        builder.Property(cfv => cfv.Value)
            .HasMaxLength(255);

        builder.HasOne(cfv => cfv.CustomField)
            .WithMany(cf => cf.FieldValues)
            .HasForeignKey(cfv => cfv.CustomFieldId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cfv => cfv.Issue)
            .WithMany(i => i.CustomValues)
            .HasForeignKey(cfv => cfv.IssueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
