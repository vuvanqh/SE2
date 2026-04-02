using Microsoft.AspNetCore.Identity;
using StudentPlanner.Infrastructure.IdentityEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Infrastructure;

public class ApplicationRole : IdentityRole<Guid>
{
}
