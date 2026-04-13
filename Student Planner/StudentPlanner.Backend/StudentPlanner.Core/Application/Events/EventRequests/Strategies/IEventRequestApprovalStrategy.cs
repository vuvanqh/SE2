using System.Threading.Tasks;
using StudentPlanner.Core.Domain;

namespace StudentPlanner.Core.Application.EventRequests.Strategies;

public interface IEventRequestApprovalStrategy
{
    RequestType RequestType { get; }
    Task ExecuteAsync(EventRequest eventRequest);
}
