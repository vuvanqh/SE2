using System.Threading.Tasks;
using StudentPlanner.Core.Domain;

namespace StudentPlanner.Core.Application.EventRequests.Strategies;

public interface IEventRequestApprovalStrategy
{
    Task ExecuteAsync(EventRequest eventRequest);
}
