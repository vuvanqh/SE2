using Microsoft.EntityFrameworkCore;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;

namespace StudentPlanner.Infrastructure.Repositories;

public class AcademicEventRepository : IAcademicEventRepository
{
    private readonly ApplicationDbContext _context;

    public AcademicEventRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(AcademicEvent academicEvent)
    {
        await _context.AcademicEvents.AddAsync(academicEvent);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid eventId)
    {
        AcademicEvent? e = await _context.AcademicEvents.FirstOrDefaultAsync(e => e.Id == eventId);
        if (e == null) return;

        _context.AcademicEvents.Remove(e);
        await _context.SaveChangesAsync();
    }

    public async Task<AcademicEvent?> GetByIdAsync(Guid eventId)
    {
        return await _context.AcademicEvents
            .FirstOrDefaultAsync(e => e.Id == eventId);
    }

    public async Task<IEnumerable<AcademicEvent>> GetAllAsync()
    {
        return await _context.AcademicEvents
            .ToListAsync();
    }

    public async Task<IEnumerable<AcademicEvent>> GetByFacultyIdAsync(Guid facultyId)
    {
        return await _context.AcademicEvents
            .Where(e => e is FacultyEvent && e.FacultyId == facultyId)
            .ToListAsync();
    }

    public async Task<IEnumerable<AcademicEvent>> GetUniversityEventsAsync()
    {
        return await _context.AcademicEvents
            .Where(e => e is UniversityEvent)
            .ToListAsync();
    }

    public async Task UpdateAsync(AcademicEvent academicEvent)
    {
        await _context.SaveChangesAsync();
    }

    public async Task<HashSet<Guid>> GetSubscribedEventIdsAsync(Guid userId)
    {
        return await _context.AcademicEventSubscribers
            .Where(s => s.UserId == userId)
            .Select(s => s.AcademicEventId)
            .ToHashSetAsync();
    }

    public async Task<bool> IsSubscribedAsync(Guid eventId, Guid userId)
    {
        return await _context.AcademicEventSubscribers
            .AnyAsync(s => s.AcademicEventId == eventId && s.UserId == userId);
    }

    public async Task SubscribeAsync(Guid eventId, Guid userId)
    {
        bool alreadySubscribed = await IsSubscribedAsync(eventId, userId);
        if (alreadySubscribed)
            return;

        await _context.AcademicEventSubscribers.AddAsync(new AcademicEventSubscriber
        {
            AcademicEventId = eventId,
            UserId = userId
        });

        await _context.SaveChangesAsync();
    }

    public async Task UnsubscribeAsync(Guid eventId, Guid userId)
    {
        var subscription = await _context.AcademicEventSubscribers
            .FirstOrDefaultAsync(s => s.AcademicEventId == eventId && s.UserId == userId);

        if (subscription == null)
            return;

        _context.AcademicEventSubscribers.Remove(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AcademicEvent>> GetByFacultiesAsync(List<Guid> facultyIds)
    {
        return await _context.AcademicEvents
            .Where(e => e is FacultyEvent && e.FacultyId.HasValue && facultyIds.Contains(e.FacultyId.Value))
            .ToListAsync();
    }
}
