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
        return await _context.AcademicEvents.FirstOrDefaultAsync(e => e.Id == eventId);
    }

    public async Task UpdateAsync(AcademicEvent academicEvent)
    {
        _context.AcademicEvents.Update(academicEvent);
        await _context.SaveChangesAsync();
    }
}
