namespace FamilyRelocation.Application.Common.Models;

/// <summary>
/// A paginated list of items with metadata for pagination.
/// </summary>
public class PaginatedList<T>
{
    /// <summary>
    /// The items on the current page.
    /// </summary>
    public List<T> Items { get; }

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// Indicates if there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Indicates if there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Initializes a new instance of the paginated list.
    /// </summary>
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
