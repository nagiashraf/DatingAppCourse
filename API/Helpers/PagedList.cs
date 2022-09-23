using Microsoft.EntityFrameworkCore;

namespace API.Helpers;

public class PagedList<T> : List<T>
{
    public int PageIndex { get; private set; }
    public int TotalPagesCount { get; private set; }
    public int PageSize { get; set; }
    public int TotalUsersCount { get; set; }

    public PagedList(IEnumerable<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPagesCount = (int)Math.Ceiling((double)count / pageSize);
        PageSize = pageSize;
        TotalUsersCount = count;

        this.AddRange(items);
    }

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return(new PagedList<T>(items, count, pageIndex, pageSize));
    }
}