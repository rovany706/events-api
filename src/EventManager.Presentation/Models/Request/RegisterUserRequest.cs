using System.ComponentModel.DataAnnotations;

using EventManager.Domain.Entities.Users;

namespace EventManager.Presentation.Models.Request;

/// <summary>
/// Запрос на регистрацию пользователя
/// </summary>
public record RegisterUserRequest
{
    /// <summary>
    /// Логин
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Login { get; init; }

    /// <summary>
    /// Пароль
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Password { get; init; }

    /// <summary>
    /// Роль
    /// </summary>
    public UserRole UserRole { get; init; } = UserRole.User;
}