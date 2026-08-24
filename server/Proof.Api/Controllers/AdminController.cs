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
    private readonly IngredientFlavorTagSyncService _flavorTagSyncService;

    public AdminController(
        CocktailDbSyncService syncService,
        IngredientFlavorTagSyncService flavorTagSyncService)
    {
        _syncService = syncService;
        _flavorTagSyncService = flavorTagSyncService;
    }

    [HttpPost("sync-cocktails")]
    public async Task<IActionResult> SyncCocktails()
    {
        var cocktailsAdded = await _syncService.SyncAllCocktailsAsync();
        return Ok(new { cocktailsAdded });
    }

    [HttpPost("tag-ingredient-flavors")]
    public async Task<IActionResult> TagIngredientFlavors()
    {
        var tagsAdded = await _flavorTagSyncService.TagAllIngredientsAsync();
        return Ok(new { tagsAdded });
    }
}
