using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentPlanner.Core.Domain;
using StudentPlanner.Infrastructure.IdentityEntities;

namespace StudentPlanner.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public virtual DbSet<PersonalEvent> PersonalEvents => Set<PersonalEvent>();
    public virtual DbSet<EventRequest> EventRequests => Set<EventRequest>();
    public virtual DbSet<AppFaculty> Faculties => Set<AppFaculty>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
