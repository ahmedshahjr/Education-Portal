using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class AccountRoleConfiguration : IEntityTypeConfiguration<AccountRole>
{
    public void Configure(EntityTypeBuilder<AccountRole> builder)
    {
        // An account that is removed, or a role that is deleted, takes its grants with it.
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<SchoolRole>().WithMany().HasForeignKey(a => a.SchoolRoleId).OnDelete(DeleteBehavior.Cascade);

        // A role is given to an account once.
        builder.HasIndex(a => new { a.SchoolId, a.UserId, a.SchoolRoleId }).IsUnique();
    }
}
