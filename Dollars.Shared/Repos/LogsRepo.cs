using Dapper;
using Microsoft.Extensions.Configuration;

public class LogsRepo : BaseRepo
{

    public LogsRepo(IConfiguration config) : base(config)
    {
        
    }

    public async Task<List<LogRow>> Page(int page, int count)
    {
        var sql = "select * from Logs order by Id desc offset (@page-1) * @count rows fetch next @count rows only";
        var rv = await WithConnAsync(null, c => c.QueryAsync<LogRow>(sql, new { page, count }));
        return rv.ToList();
    }
}