namespace FamilyRelocation.Application.Common.Models;

/// <summary>
/// A paginated list of items with metadata for pagination.
/// </summary>
public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PaginatedList(List<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    /// <summary>
    /// Create an empty paginated list.
    /// </summary>
    public static PaginatedList<T> Empty(int page, int pageSize)
    {
        return new PaginatedList<T>(new List<T>(), 0, page, pageSize);
    }
}
