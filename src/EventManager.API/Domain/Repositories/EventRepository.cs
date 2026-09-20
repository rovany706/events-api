using EventManager.API.Domain.DataAccess;
using EventManager.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventManager.API.Domain.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _dbContext;

    public EventRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public IQueryable<Event> GetEvents()
    {
        return _dbContext.Events.AsQueryable().AsNoTracking();
    }

    public Task<Event?> GetEventByIdAsync(int id, CancellationToken ct)
    {
        return _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public Task AddEventAsync(Event eventToAdd, CancellationToken ct)
    {
        return _dbContext.Events.AddAsync(eventToAdd, ct).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _dbContext.SaveChangesAsync(ct);
    }

    public void RemoveEvent(Event eventToRemove)
    {
        _dbContext.Events.Remove(eventToRemove);
    }
}