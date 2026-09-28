using EventManager.API.Domain.DataAccess;
using EventManager.API.Domain.Repositories.Interfaces;
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
    
    /// <inheritdoc />
    public IQueryable<Event> GetEvents()
    {
        return _dbContext.Events.AsQueryable().AsNoTracking();
    }

    /// <inheritdoc />
    public Task<Event?> GetEventByIdAsync(int id, CancellationToken ct)
    {
        return _dbContext.Events.Include(e => e.Bookings).FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    /// <inheritdoc />
    public Task AddEventAsync(Event eventToAdd, CancellationToken ct)
    {
        return _dbContext.Events.AddAsync(eventToAdd, ct).AsTask();
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _dbContext.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public void RemoveEvent(Event eventToRemove)
    {
        _dbContext.Events.Remove(eventToRemove);
    }
}