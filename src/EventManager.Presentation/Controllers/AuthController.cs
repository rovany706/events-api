using Asp.Versioning;

using EventManager.Application.Abstractions.Services;
using EventManager.Application.Abstractions.Services.Dto;
using EventManager.Application.Common.Results;
using EventManager.Presentation.Models.Request;

using Microsoft.AspNetCore.Mvc;

namespace EventManager.Presentation.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Регистрация пользователя в системе
    /// </summary>
    /// <param name="request">Запрос на регистрацию пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <response code="204">Пользователь зарегистрирован</response>
    /// <response code="400">Ошибка регистрации</response>
    [HttpPost("register")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Получен запрос на регистрацию (login = {Login})", request.Login);

        var dto = new UserCredentialsDto(request.Login, request.Password);

        var result = await _userService.RegisterUserAsync(dto, request.UserRole, cancellationToken);

        if (!result.IsSuccess)
            return Problem(detail: result.Error!.ErrorMessage, statusCode: result.Error.GetHttpStatusCodeForError());

        return NoContent();
    }

    /// <summary>
    /// Вход пользователя в систему
    /// </summary>
    /// <param name="request">Запрос на вход пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <response code="200">Успешный вход, возвращается JWT-токен</response>
    /// <response code="400">Ошибка входа</response>
    [HttpPost("login")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> LoginAsync(LoginUserRequest request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Получен запрос на вход (login = {Login})", request.Login);

        var dto = new UserCredentialsDto(request.Login, request.Password);

        var result = await _userService.LoginUserAsync(dto, cancellationToken);

        if (!result.IsSuccess)
            return Problem(detail: result.Error!.ErrorMessage, statusCode: result.Error.GetHttpStatusCodeForError());

        return Ok(result.Value);
    }
}