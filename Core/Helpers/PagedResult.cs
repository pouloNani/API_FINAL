namespace Core.Helpers;

public class PagedResult<T>
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
    public IReadOnlyList<T> Data { get; set; } = [];

    public static PagedResult<T> Create(IReadOnlyList<T> data, int totalCount, PaginationParams pagination)
    {
        return new PagedResult<T>
        {
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize,
            TotalCount = totalCount,
            Data = data
        };
    }
}