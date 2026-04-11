using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPlanner.Core.Domain;
using StudentPlanner.Infrastructure.IdentityEntities;

namespace StudentPlanner.Infrastructure.EntityConfigurations;

public class AcademicEventConfig : IEntityTypeConfiguration<AcademicEvent>
{
    public void Configure(EntityTypeBuilder<AcademicEvent> builder)
    {
        builder.ToTable("AcademicEvents");
        builder.HasKey(e => e.Id);

        builder.HasOne<AppFaculty>()
            .WithMany()
            .HasForeignKey(e => e.FacultyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(e => e.EventDetails);

        builder.HasIndex(e => e.FacultyId);
    }
}
