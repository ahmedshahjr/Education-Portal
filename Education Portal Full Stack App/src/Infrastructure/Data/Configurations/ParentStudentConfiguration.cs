using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class ParentStudentConfiguration : IEntityTypeConfiguration<ParentStudent>
{
    public void Configure(EntityTypeBuilder<ParentStudent> builder)
    {
        builder.HasOne<UserDetails>().WithMany().HasForeignKey(p => p.ParentUserDetailsId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UserDetails>().WithMany().HasForeignKey(p => p.StudentUserDetailsId).OnDelete(DeleteBehavior.Restrict);

        // A parent is linked to a student once, and the school's children of a parent are listed by the parent.
        builder.HasIndex(p => new { p.SchoolId, p.ParentUserDetailsId, p.StudentUserDetailsId }).IsUnique();
        builder.HasIndex(p => new { p.SchoolId, p.StudentUserDetailsId });
    }
}
