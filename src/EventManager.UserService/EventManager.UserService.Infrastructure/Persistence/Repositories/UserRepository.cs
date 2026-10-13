using EventManager.UserService.Application.Abstractions.Persistence.Repositories;
using EventManager.UserService.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace EventManager.UserService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Репозиторий пользователей
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly UserDbContext _dbContext;

    public UserRepository(UserDbContext dbContext)
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