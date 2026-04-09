using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPlanner.Core.Domain;
using StudentPlanner.Infrastructure.IdentityEntities;

namespace StudentPlanner.Infrastructure.EntityConfigurations;

public class AcademicEventSubscriberConfig : IEntityTypeConfiguration<AcademicEventSubscriber>
{
    public void Configure(EntityTypeBuilder<AcademicEventSubscriber> builder)
    {
        builder.ToTable("AcademicEventSubscribers");
        builder.HasKey(s => new { s.AcademicEventId, s.UserId });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder.HasOne<AcademicEvent>()
            .WithMany(e => e.Subscribers)
            .HasForeignKey(s => s.AcademicEventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
