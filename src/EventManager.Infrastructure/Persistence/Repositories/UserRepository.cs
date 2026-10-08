using EventManager.Application.Abstractions.Persistence.Repositories;
using EventManager.Domain.Entities.Users;

using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure.Persistence.Repositories;

/// <summary>
/// Репозиторий пользователей
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<User?> GetUserByLoginAsync(string login, CancellationToken cancellationToken)
    {
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Login == login, cancellationToken);
    }

    /// <inheritdoc />
    public Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken)
    {
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    /// <inheritdoc />
    public Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        return _dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}