using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BugCore.Core.Entities;

namespace BugCore.Infrastructure.Data.Configurations;

public class IssueAttachmentConfiguration : IEntityTypeConfiguration<IssueAttachment>
{
    public void Configure(EntityTypeBuilder<IssueAttachment> builder)
    {
        builder.ToTable("BugCoreIssueAttachments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(a => a.FileType)
            .HasMaxLength(250);

        builder.Property(a => a.Title)
            .HasMaxLength(250);

        builder.Property(a => a.Description)
            .HasMaxLength(250);

        builder.HasOne(a => a.Issue)
            .WithMany(i => i.Attachments)
            .HasForeignKey(a => a.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
