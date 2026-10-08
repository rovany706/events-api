using EventManager.Domain.Entities.Bookings;

namespace EventManager.Domain.Entities.Users;

/// <summary>
/// Пользователь
/// </summary>
public class User
{
    private const int UndefinedId = 0;

    private User()
    {
        Login = null!;
        PasswordHash = null!;
    }

    private User(int id, string login, string passwordHash, UserRole role)
    {
        Id = id;
        Login = login;
        PasswordHash = passwordHash;
        Role = role;
    }

    private User(string login, string passwordHash, UserRole role) : this(UndefinedId, login, passwordHash, role)
    {
    }

    public static User CreateInstance(int id, string login, string passwordHash, UserRole role)
    {
        return new User(id, login, passwordHash, role);
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

    /// <summary>
    /// Бронирования пользователя
    /// </summary>
    public List<Booking> Bookings { get; private set; } = [];
}