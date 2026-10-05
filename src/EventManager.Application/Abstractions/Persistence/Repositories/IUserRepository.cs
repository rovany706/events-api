using EventManager.Domain.Entities.Users;

namespace EventManager.Application.Abstractions.Persistence.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken);

    Task<int> GetActiveBookingCount(int userId, CancellationToken cancellationToken);
}