namespace EventManager.API.Models.Response;

/// <summary>
/// Страница результатов
/// </summary>
/// <typeparam name="T">Тип результатов</typeparam>
/// <param name="Items">Результаты</param>
/// <param name="ItemCount">Количество результатов на текущей странице</param>
/// <param name="CurrentPage">Номер текущей страницы</param>
/// <param name="TotalPages">Общее количество страниц</param>
/// <param name="TotalItems">Общее количество результатов</param>
public record PaginatedResult<T>(IReadOnlyCollection<T> Items, int ItemCount, int CurrentPage, int TotalPages, int TotalItems);
