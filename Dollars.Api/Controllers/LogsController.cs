using Dollars.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = AppRoles.Admin)]
public class LogsController : Controller
{
    public LogsController(ILogger<LogsController> logger)
    {
        
    }

    [HttpGet("{page:int}")]
    public async Task<List<LogRow>> Get(int page)
    {
        
    }
}