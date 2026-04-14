using Microsoft.EntityFrameworkCore;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;

namespace StudentPlanner.Infrastructure.Repositories.Events;

public class UsosEventRepository : IUsosEventRepository
{
    private readonly ApplicationDbContext _context;

    public UsosEventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task DeleteByUserAndRangeAsync(Guid userId, DateTime from, DateTime to)
    {
        var events = await _context.StudentUsosEvents
            .Where(x => x.UserId == userId &&
                        x.EventDetails.StartTime < to &&
                        x.EventDetails.EndTime > from)
            .ToListAsync();

        _context.StudentUsosEvents.RemoveRange(events);
    }

    public async Task AddRangeAsync(IEnumerable<UsosEvent> events)
    {
        await _context.StudentUsosEvents.AddRangeAsync(events);
    }

    public async Task<List<UsosEvent>> GetByUserAndRangeAsync(Guid userId, DateTime from, DateTime to)
    {
        return await _context.StudentUsosEvents
            .Where(x => x.UserId == userId &&
                        x.EventDetails.StartTime < to &&
                        x.EventDetails.EndTime > from)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}