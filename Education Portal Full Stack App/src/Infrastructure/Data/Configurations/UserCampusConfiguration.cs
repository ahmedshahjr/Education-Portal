using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class UserCampusConfiguration : IEntityTypeConfiguration<UserCampus>
{
    public void Configure(EntityTypeBuilder<UserCampus> builder)
    {
        builder.ToTable("UserCampuses");

        // A deleted campus takes its assignments with it; a person is soft deleted, so their rows stay for the audit.
        builder.HasOne<School>().WithMany().HasForeignKey(u => u.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<UserDetails>().WithMany().HasForeignKey(u => u.UserDetailsId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Campus>().WithMany().HasForeignKey(u => u.CampusId).OnDelete(DeleteBehavior.Cascade);

        // A person is assigned to a campus once.
        builder.HasIndex(u => new { u.UserDetailsId, u.CampusId }).IsUnique();
        builder.HasIndex(u => u.SchoolId);
        builder.HasIndex(u => u.CampusId);
    }
}
