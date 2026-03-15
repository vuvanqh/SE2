using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPlanner.Core.Domain;
using StudentPlanner.Infrastructure.IdentityEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Infrastructure.EntityConfigurations;

public class PersonalEventConfig : IEntityTypeConfiguration<PersonalEvent>
{
    public void Configure(EntityTypeBuilder<PersonalEvent> builder)
    {
        builder.ToTable("PersonalEvents");
        builder.HasKey(e => e.Id);


        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.UserId);

        builder.OwnsOne(e => e.EventDetails);

        builder.HasIndex(e => e.UserId);
    }
}
