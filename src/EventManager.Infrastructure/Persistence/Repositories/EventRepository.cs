using EventManager.Application.Abstractions.Persistence.Repositories;
using EventManager.Application.Abstractions.Services.Dto;
using EventManager.Application.Common.Pagination;
using EventManager.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure.Persistence.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _dbContext;

    public EventRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResult<Event>> GetEvents(EventFilterDto? filterDto = null,
        PaginationParamsDto? paginationParams = null,
        CancellationToken ct = default)
    {
        var events = _dbContext.Events.AsNoTracking().AsQueryable();

        if (filterDto != null)
        {
            events = FilterEvents(events, filterDto);
        }

        if (paginationParams != null)
        {
            return await PaginateResults(events, paginationParams, ct);
        }

        var materializedEvents = await events.ToListAsync(ct);

        return new PaginatedResult<Event>(materializedEvents, 1, 1, 1, materializedEvents.Count);
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

    private static IQueryable<Event> FilterEvents(IQueryable<Event> events, EventFilterDto filterDto)
    {
        if (!string.IsNullOrWhiteSpace(filterDto.Title))
        {
            events = events.Where(e => EF.Functions.ILike(e.Title, $"%{filterDto.Title}%"));
        }

        if (filterDto.From.HasValue)
        {
            events = events.Where(e => e.StartAt >= filterDto.From.Value);
        }

        if (filterDto.To.HasValue)
        {
            events = events.Where(e => e.EndAt <= filterDto.To.Value);
        }

        return events;
    }

    private static async Task<PaginatedResult<Event>> PaginateResults(IQueryable<Event> events,
        PaginationParamsDto paginationParams, CancellationToken ct)
    {
        var count = await events.CountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)count / paginationParams.PageSize);
        var eventPage = await events
            .OrderBy(e => e.Id)
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(ct);

        return new PaginatedResult<Event>(eventPage, eventPage.Count, paginationParams.Page, totalPages, count);
    }
}