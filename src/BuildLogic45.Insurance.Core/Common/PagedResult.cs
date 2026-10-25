namespace BuildLogic45.Insurance.Core.Common;

/// <summary>A single page of results from a search, plus the total number of matches.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
