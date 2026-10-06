using System.ComponentModel.DataAnnotations;

namespace EventManager.Presentation.Models.Request;

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
    public string Login { get; init; }

    /// <summary>
    /// Пароль
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Password { get; init; }
}