using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proof.Api.Services;

namespace Proof.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly CocktailDbSyncService _syncService;

    public AdminController(CocktailDbSyncService syncService)
    {
        _syncService = syncService;
    }

    [HttpPost("sync-cocktails")]
    public async Task<IActionResult> SyncCocktails()
    {
        var cocktailsAdded = await _syncService.SyncAllCocktailsAsync();
        return Ok(new { cocktailsAdded });
    }
}
