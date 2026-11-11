using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class SchoolRequestConfiguration : IEntityTypeConfiguration<SchoolRequest>
{
    public void Configure(EntityTypeBuilder<SchoolRequest> builder)
    {
        builder.Property(r => r.SchoolName).IsRequired().HasMaxLength(SchoolRequest.SchoolNameMaxLength);
        builder.Property(r => r.ContactName).IsRequired().HasMaxLength(SchoolRequest.ContactNameMaxLength);
        builder.Property(r => r.ContactEmail).IsRequired().HasMaxLength(SchoolRequest.ContactEmailMaxLength);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(16);

        builder.HasIndex(r => r.Status);
    }
}
