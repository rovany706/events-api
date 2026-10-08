namespace EventManager.Application.Abstractions.Security;

/// <summary>
/// Интерфейс хеширования паролей
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Получить хеш пароля
    /// </summary>
    /// <param name="password">Пароль</param>
    string Hash(string password);

    /// <summary>
    /// Проверить пароль с ожидаемым хешом
    /// </summary>
    /// <param name="password">Пароль</param>
    /// <param name="expectedHash">Ожидаемый хеш</param>
    bool Verify(string password, string expectedHash);
}