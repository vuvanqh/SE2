using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StudentPlanner.Infrastructure.IdentityEntities;

public class FacultyConfig : IEntityTypeConfiguration<AppFaculty>
{
    public void Configure(EntityTypeBuilder<AppFaculty> builder)
    {
        builder.HasKey(e => e.Id);



        builder.HasIndex(e => e.FacultyId);
        builder.HasIndex(e => e.FacultyCode);
    }
}
