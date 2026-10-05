namespace EventManager.Application.Common.Pagination;

/// <summary>
/// DTO пагинации
/// </summary>
/// <param name="Page">Номер страницы</param>
/// <param name="PageSize">Размер страницы</param>
public record PaginationParamsDto(int Page, int PageSize);