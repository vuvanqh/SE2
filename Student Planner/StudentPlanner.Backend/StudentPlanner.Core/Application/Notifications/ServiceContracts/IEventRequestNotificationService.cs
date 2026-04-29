using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Application.Notifications.ServiceContracts;

public interface IEventRequestNotificationService
{
    Task EventRequestUpdated(Guid managerId);
    Task NotifyEventRequestListChanged();
    Task AcademicEventNotification(Guid studentId, string message, Guid? facultyId);
}
