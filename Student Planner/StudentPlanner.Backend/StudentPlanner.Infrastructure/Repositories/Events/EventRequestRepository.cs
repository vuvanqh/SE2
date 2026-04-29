using Microsoft.EntityFrameworkCore;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;

namespace StudentPlanner.Infrastructure.Repositories;

public class EventRequestRepository : IEventRequestRepository
{
    private readonly ApplicationDbContext _context;

    public EventRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventRequest>> GetAllAsync()
    {
        return await _context.EventRequests
            .ToListAsync();
    }

    public async Task<List<EventRequest>> GetByFacultyIdAsync(Guid facultyId)
    {
        return await _context.EventRequests
            .Where(e => e.FacultyId == facultyId)
            .ToListAsync();
    }

    public async Task<List<EventRequest>> GetByManagerIdAsync(Guid managerId)
    {
        return await _context.EventRequests
            .Where(e => e.ManagerId == managerId)
            .ToListAsync();
    }

    public async Task<EventRequest?> GetByIdAsync(Guid requestId)
    {
        return await _context.EventRequests
            .FirstOrDefaultAsync(e => e.Id == requestId);
    }

    public async Task AddAsync(EventRequest eventRequest)
    {
        await _context.EventRequests.AddAsync(eventRequest);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(EventRequest eventRequest)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid requestId)
    {
        EventRequest? eventRequest = await _context.EventRequests
            .FirstOrDefaultAsync(e => e.Id == requestId);
        if (eventRequest == null)
        {
            return;
        }
        _context.EventRequests.Remove(eventRequest);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteByManagerIdAsync(Guid managerId)
    {
        var requests = await _context.EventRequests
            .Where(e => e.ManagerId == managerId)
            .ToListAsync();
        _context.EventRequests.RemoveRange(requests);
        await _context.SaveChangesAsync();
    }
}
