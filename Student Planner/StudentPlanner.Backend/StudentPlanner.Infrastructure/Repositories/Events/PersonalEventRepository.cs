using Microsoft.EntityFrameworkCore;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;

namespace StudentPlanner.Infrastructure.Repositories;

public class PersonalEventRepository : IPersonalEventRepository
{
    private readonly ApplicationDbContext _context;
    public PersonalEventRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(PersonalEvent personalEvent)
    {
        await _context.PersonalEvents.AddAsync(personalEvent);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid eventId)
    {
        PersonalEvent? e = await _context.PersonalEvents.FirstOrDefaultAsync(e => e.Id == eventId);
        if (e == null) return;

        _context.PersonalEvents.Remove(e);
        await _context.SaveChangesAsync();
    }

    public async Task<PersonalEvent?> GetEventByEventIdAsync(Guid eventId)
    {
        PersonalEvent? e = await _context.PersonalEvents.FirstOrDefaultAsync(e => e.Id == eventId);
        return e;
    }

    public async Task<List<PersonalEvent>> GetEventsByUserIdAsync(Guid userId)
    {
        return await _context.PersonalEvents.Where(e => e.UserId == userId).ToListAsync();
    }

    public async Task UpdateAsync(PersonalEvent personalEvent)
    {
        _context.PersonalEvents.Update(personalEvent);
        await _context.SaveChangesAsync();
    }
}
