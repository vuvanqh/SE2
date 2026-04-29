using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.Entities;
using StudentPlanner.Infrastructure.IdentityEntities;

namespace StudentPlanner.Infrastructure.EntityConfigurations;

public class AcademicEventConfig : IEntityTypeConfiguration<AcademicEvent>
{
    public void Configure(EntityTypeBuilder<AcademicEvent> builder)
    {
        builder.ToTable("AcademicEvents");
        builder.HasKey(e => e.Id);

        builder.HasDiscriminator<string>("EventType")
            .HasValue<FacultyEvent>("Faculty")
            .HasValue<UniversityEvent>("University");

        builder.Property(e => e.FacultyId)
            .HasColumnName("FacultyId");

        builder.OwnsOne(e => e.EventDetails, od =>
        {
            od.Property(p => p.Title).HasMaxLength(50).IsRequired();
            od.Property(p => p.Description);
            od.Property(p => p.StartTime).IsRequired();
            od.Property(p => p.EndTime).IsRequired();
            od.Property(p => p.Location).HasMaxLength(70);
        });

        builder.HasOne<AppFaculty>()
            .WithMany()
            .HasForeignKey(e => e.FacultyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.FacultyId);
    }
}
