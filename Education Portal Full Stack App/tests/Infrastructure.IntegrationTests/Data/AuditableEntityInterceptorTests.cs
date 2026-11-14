using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace EducationPortal.Infrastructure.IntegrationTests.Data;

/// <summary>The audit fields of a row: the signed-in user is recorded, and with nobody signed in the system account is (never null).</summary>
public class AuditableEntityInterceptorTests
{
    [Test]
    public void ShouldRecordTheSystemAccountAsCreatorWhenNoOneIsSignedIn()
    {
        using var context = new SampleContext();
        var row = new Sample();
        context.Add(row);

        new AuditableEntityInterceptor(new FakeUser(id: null), TimeProvider.System).UpdateEntities(context);

        row.CreatedBy.ShouldBe(SystemUser.Id);
        row.LastModifiedBy.ShouldBe(SystemUser.Id);
    }

    [Test]
    public void ShouldRecordTheSignedInUserAsCreatorAndModifier()
    {
        using var context = new SampleContext();
        var row = new Sample();
        context.Add(row);

        new AuditableEntityInterceptor(new FakeUser(id: "user-42"), TimeProvider.System).UpdateEntities(context);

        row.CreatedBy.ShouldBe("user-42");
        row.LastModifiedBy.ShouldBe("user-42");
    }

    [Test]
    public void ShouldKeepTheCreatorWhenAnotherUserModifiesTheRow()
    {
        using var context = new SampleContext();
        var row = new Sample { Id = 7, CreatedBy = "creator", LastModifiedBy = "creator" };
        var entry = context.Attach(row);
        entry.Property(r => r.Note).CurrentValue = "changed";
        entry.Property(r => r.Note).IsModified = true;

        new AuditableEntityInterceptor(new FakeUser(id: "editor"), TimeProvider.System).UpdateEntities(context);

        row.CreatedBy.ShouldBe("creator");
        row.LastModifiedBy.ShouldBe("editor");
    }

    [Test]
    public void ShouldRecordTheSystemAccountAsModifierWhenNoOneIsSignedIn()
    {
        using var context = new SampleContext();
        var row = new Sample { Id = 7, CreatedBy = "creator", LastModifiedBy = "creator" };
        var entry = context.Attach(row);
        entry.Property(r => r.Note).CurrentValue = "changed";
        entry.Property(r => r.Note).IsModified = true;

        new AuditableEntityInterceptor(new FakeUser(id: null), TimeProvider.System).UpdateEntities(context);

        row.LastModifiedBy.ShouldBe(SystemUser.Id);
    }

    private sealed class FakeUser(string? id) : IUser
    {
        public string? Id { get; } = id;

        public List<string>? Roles => null;

        public bool MustChangePassword => false;
    }

    private sealed class Sample : BaseAuditableEntity
    {
        public string? Note { get; set; }
    }

    /// <summary>Only tracks changes; it never opens a connection.</summary>
    private sealed class SampleContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseNpgsql("Host=unused");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sample>().Ignore(s => s.DomainEvents);
        }
    }
}
