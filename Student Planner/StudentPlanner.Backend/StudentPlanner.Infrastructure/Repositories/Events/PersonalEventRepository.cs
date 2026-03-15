using Microsoft.EntityFrameworkCore;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;

namespace StudentPlanner.Infrastructure.Repositories;

public class PersonalEventRepository : IPersonalEventRepository
{
    private readonly ApplicationDbContext _context;
    public PersonalEventRepository(ApplicationDbContext context) => _context = context;

    public async Task DeletePersonalEventAsync(Guid eventId)
    {
        PersonalEvent? e = await _context.PersonalEvents.FirstOrDefaultAsync(e => e.Id == eventId);
        if (e == null) return;

        _context.PersonalEvents.Remove(e);
        await _context.SaveChangesAsync();
    }

    public async Task<PersonalEvent?> GetPersonalEventByIdAsync(Guid eventId)
    {
        PersonalEvent? e = await _context.PersonalEvents.FirstOrDefaultAsync( e => e.Id== eventId);
        return e;
    }

    public async Task<List<PersonalEvent>> GetPersonalEventsByUserIdAsync(Guid userId)
    {
        return await _context.PersonalEvents.Where(e=>e.UserId == userId).ToListAsync();
    }

    public async Task UpdatePersonalEventAsync(PersonalEvent personalEvent)
    {
        _context.PersonalEvents.Update(personalEvent);
        await _context.SaveChangesAsync();
    }
}
