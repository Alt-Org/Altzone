/// <summary>
/// Pagination data received from server.
/// </summary>
public class PaginationData
{
    public int currentPage { get; set; }
    public int limit { get; set; }
    public int offset { get; set; }
    public int itemCount { get; set; }
    public int pageCount { get; set; }
}
