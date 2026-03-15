using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Domain;

public class PersonalEvent: Event
{
    public Guid UserId { get; set; }
}
