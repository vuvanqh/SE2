using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPlanner.Core.Domain;
using StudentPlanner.Infrastructure.IdentityEntities;

namespace StudentPlanner.Infrastructure.EntityConfigurations;

public class EventRequestConfig : IEntityTypeConfiguration<EventRequest>
{
    public void Configure(EntityTypeBuilder<EventRequest> builder)
    {
        builder.ToTable("EventRequests");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.RequestType)
            .HasConversion<string>();

        builder.Property(e => e.Status)
            .HasConversion<string>();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.OwnsOne(e => e.EventDetails);

        builder.HasOne<AppFaculty>()
            .WithMany()
            .HasForeignKey(e => e.FacultyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.ReviewedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => e.ManagerId);
        builder.HasIndex(e => e.Status);
    }
}
