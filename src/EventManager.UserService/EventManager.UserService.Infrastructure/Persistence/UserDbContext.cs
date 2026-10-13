using EventManager.UserService.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace EventManager.UserService.Infrastructure.Persistence;

public class UserDbContext : DbContext
{
    /// <inheritdoc />
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
    }
}