using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BugCore.Core.Entities;

namespace BugCore.Infrastructure.Data.Configurations;

public class CustomFieldConfiguration : IEntityTypeConfiguration<CustomField>
{
    public void Configure(EntityTypeBuilder<CustomField> builder)
    {
        builder.ToTable("BugCoreCustomFields");
        builder.HasKey(cf => cf.Id);

        builder.Property(cf => cf.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(cf => cf.Name)
            .IsUnique();

        builder.Property(cf => cf.PossibleValues)
            .HasMaxLength(1000);

        builder.Property(cf => cf.DefaultValue)
            .HasMaxLength(255);

        builder.Property(cf => cf.ValidRegexp)
            .HasMaxLength(255);

        // Explicitly ignore helper properties to prevent EF Core from treating them as SQL columns
        builder.Ignore(cf => cf.RegularExpression);
        builder.Ignore(cf => cf.ReadAccessLevel);
        builder.Ignore(cf => cf.WriteAccessLevel);
        builder.Ignore(cf => cf.DisplayOnReport);
        builder.Ignore(cf => cf.DisplayOnUpdate);
        builder.Ignore(cf => cf.DisplayOnView);
        builder.Ignore(cf => cf.RequireOnReport);
        builder.Ignore(cf => cf.RequireOnUpdate);
    }
}
