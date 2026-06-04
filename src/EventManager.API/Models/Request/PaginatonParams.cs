using System.ComponentModel.DataAnnotations;

namespace EventManager.API.Models.Request;

/// <summary>
/// Параметры пагинации
/// </summary>
public record PaginatonParams
{
    /// <summary>
    /// Номер страницы
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    /// <summary>
    /// Размер страницы
    /// </summary>
    [Range(1, 100)]
    public int PageSize { get; init; } = 10;
}
