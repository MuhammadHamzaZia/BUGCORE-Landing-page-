using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BugCore.Core.Entities;

namespace BugCore.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("BugCoreCategories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(128);

        // In BugCoreBT, category name is unique per project (idx_category_project_name)
        builder.HasIndex(c => new { c.ProjectId, c.Name }).IsUnique();

        builder.HasOne(c => c.Project)
            .WithMany(p => p.Categories)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.DefaultAssignee)
            .WithMany()
            .HasForeignKey(c => c.DefaultAssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Issues)
            .WithOne(i => i.Category)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
