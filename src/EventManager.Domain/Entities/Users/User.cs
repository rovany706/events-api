using EventManager.Domain.Entities.Bookings;

namespace EventManager.Domain.Entities.Users;

/// <summary>
/// Пользователь
/// </summary>
public class User
{
    private User()
    {
        Login = null!;
        PasswordHash = null!;
    }

    private User(string login, string passwordHash, UserRole role)
    {
        Login = login;
        PasswordHash = passwordHash;
        Role = role;
    }

    public static User CreateInstance(string login, string passwordHash, UserRole role)
    {
        return new User(login, passwordHash, role);
    }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Логин
    /// </summary>
    public string Login { get; private set; }

    /// <summary>
    /// Хеш пароля
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; private set; }
}