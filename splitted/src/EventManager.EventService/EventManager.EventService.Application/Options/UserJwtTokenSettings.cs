using System.ComponentModel.DataAnnotations;

namespace EventManager.EventService.Application.Options;

/// <summary>
/// Настройки JWT-токена для пользователей
/// </summary>
public class UserJwtTokenSettings
{
    /// <summary>
    /// Секрет для подписи токена
    /// </summary>
    [Required]
    public required string Secret { get; init; }

    /// <summary>
    /// Издатель токена
    /// </summary>
    [Required]
    public required string Issuer { get; init; }

    /// <summary>
    /// Получатель токена
    /// </summary>
    [Required]
    public required string Audience { get; init; }

    /// <summary>
    /// Время жизни токена
    /// </summary>
    [Required]
    public required TimeSpan Lifetime { get; init; }
}