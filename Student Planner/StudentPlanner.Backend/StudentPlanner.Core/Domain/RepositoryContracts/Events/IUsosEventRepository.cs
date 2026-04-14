namespace StudentPlanner.Core.Domain.RepositoryContracts;

public interface IUsosEventRepository
{
    Task DeleteByUserAndRangeAsync(Guid userId, DateTime from, DateTime to);
    Task AddRangeAsync(IEnumerable<UsosEvent> events);
    Task<List<UsosEvent>> GetByUserAndRangeAsync(Guid userId, DateTime from, DateTime to);
    Task SaveChangesAsync();
}