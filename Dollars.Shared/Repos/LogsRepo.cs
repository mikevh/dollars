using Microsoft.Extensions.Configuration;

public class LogsRepo : BaseRepo
{

    public LogsRepo(IConfiguration config) : base(config)
    {
        
    }

    public async Task<List<LogRow>> Page(int page, int count)
    {
        
    }
}