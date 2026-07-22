namespace DTOs;

/// <summary>
/// Represents a paginated result set containing a collection of items and related metadata.
/// </summary>
/// <typeparam name="T">The type of items contained in the result set.</typeparam>
/// <remarks>
/// This class is used to encapsulate a subset of data along with additional information
/// about the total dataset, including pagination details. It provides properties to access
/// the items on the current page, total counts, and navigation helpers for previous and next pages.
/// </remarks>
public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}