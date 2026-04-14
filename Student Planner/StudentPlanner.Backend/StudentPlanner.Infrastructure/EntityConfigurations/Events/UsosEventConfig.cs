using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPlanner.Core.Domain;
using StudentPlanner.Infrastructure.IdentityEntities;

namespace StudentPlanner.Infrastructure.EntityConfigurations;

public class StudentUsosEventConfig : IEntityTypeConfiguration<UsosEvent>
{
    public void Configure(EntityTypeBuilder<UsosEvent> builder)
    {
        builder.ToTable("UsosEvents");
        builder.HasKey(e => e.Id);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(e => e.EventDetails);

        builder.Property(e => e.ExternalKey)
            .IsRequired();

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.ExternalKey }).IsUnique();
    }
}