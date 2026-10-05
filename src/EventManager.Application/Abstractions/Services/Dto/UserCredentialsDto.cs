namespace EventManager.Application.Abstractions.Services.Dto;

/// <summary>
/// Данные для входа пользователя
/// </summary>
/// <param name="Login">Логин</param>
/// <param name="Password">Пароль</param>
public record UserCredentialsDto(string Login, string Password);
