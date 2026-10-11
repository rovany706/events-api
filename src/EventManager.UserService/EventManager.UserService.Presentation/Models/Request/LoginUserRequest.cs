using System.ComponentModel.DataAnnotations;

namespace EventManager.UserService.Presentation.Models.Request;

/// <summary>
/// Запрос на вход пользователя
/// </summary>
public record LoginUserRequest
{
    /// <summary>
    /// Логин
    /// </summary>
    [Required]
    [MaxLength(10)]
    public required string Login { get; init; }

    /// <summary>
    /// Пароль
    /// </summary>
    [Required]
    [MaxLength(20)]
    public required string Password { get; init; }
}