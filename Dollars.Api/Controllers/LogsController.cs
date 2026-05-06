using Dollars.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = AppRoles.Admin)]
public class LogsController : Controller
{
    private readonly LogsRepo _repo;

    public LogsController(ILogger<LogsController> logger, LogsRepo repo)
    {
        _repo = repo;
    }

    [HttpGet("{page:int}")]
    public async Task<List<LogRow>> Get(int page)
    {
        var rv = await _repo.Page(page, 200);
        return rv;
    }
}