using System.ComponentModel.DataAnnotations;

namespace EventManager.Application.Options;

/// <summary>
/// Настройки JWT-токена для пользователей
/// </summary>
public class UserJwtTokenSettings
{
    /// <summary>
    /// Секрет для подписи токена
    /// </summary>
    [Required]
    public string Secret { get; set; }

    /// <summary>
    /// Издатель токена
    /// </summary>
    [Required]
    public string Issuer { get; set; }

    /// <summary>
    /// Получатель токена
    /// </summary>
    [Required]
    public string Audience { get; set; }

    /// <summary>
    /// Время жизни токена
    /// </summary>
    [Required]
    public TimeSpan Lifetime { get; set; }
}