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
            .OrderBy(e => e.EventDetails.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<AcademicEvent>> GetAllPagedAsync(int skip, int take)
    {
        return await _context.AcademicEvents
            .OrderBy(e => e.EventDetails.StartTime)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> CountAllAsync()
    {
        return await _context.AcademicEvents.CountAsync();
    }

    public async Task<IEnumerable<AcademicEvent>> GetByFacultyIdAsync(Guid facultyId)
    {
        return await _context.AcademicEvents
            .Where(e => e is FacultyEvent && e.FacultyId == facultyId)
            .OrderBy(e => e.EventDetails.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<AcademicEvent>> GetByFacultyIdPagedAsync(Guid facultyId, int skip, int take)
    {
        return await _context.AcademicEvents
            .Where(e => e is FacultyEvent && e.FacultyId == facultyId)
            .OrderBy(e => e.EventDetails.StartTime)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> CountByFacultyIdAsync(Guid facultyId)
    {
        return await _context.AcademicEvents.CountAsync(e => e is FacultyEvent && e.FacultyId == facultyId);
    }

    public async Task<IEnumerable<AcademicEvent>> GetUniversityEventsAsync()
    {
        return await _context.AcademicEvents
            .Where(e => e is UniversityEvent)
            .OrderBy(e => e.EventDetails.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<AcademicEvent>> GetUniversityEventsPagedAsync(int skip, int take)
    {
        return await _context.AcademicEvents
            .Where(e => e is UniversityEvent)
            .OrderBy(e => e.EventDetails.StartTime)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> CountUniversityEventsAsync()
    {
        return await _context.AcademicEvents.CountAsync(e => e is UniversityEvent);
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
            .OrderBy(e => e.EventDetails.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<AcademicEvent>> GetByFacultiesPagedAsync(List<Guid> facultyIds, int skip, int take)
    {
        return await _context.AcademicEvents
            .Where(e => e is FacultyEvent && e.FacultyId.HasValue && facultyIds.Contains(e.FacultyId.Value))
            .OrderBy(e => e.EventDetails.StartTime)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> CountByFacultiesAsync(List<Guid> facultyIds)
    {
        return await _context.AcademicEvents.CountAsync(e => e is FacultyEvent && e.FacultyId.HasValue && facultyIds.Contains(e.FacultyId.Value));
    }
}
