using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proof.Api.Services;

namespace Proof.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly CocktailDbSyncService _syncService;
    private readonly IngredientFlavorTagSyncService _flavorTagSyncService;
    private readonly CocktailFlavorTagSyncService _cocktailFlavorTagSyncService;
    private readonly IngredientSpiritSyncService _ingredientSpiritSyncService;
    private readonly IngredientSubstitutionSeedService _substitutionSeedService;

    public AdminController(
        CocktailDbSyncService syncService,
        IngredientFlavorTagSyncService flavorTagSyncService,
        CocktailFlavorTagSyncService cocktailFlavorTagSyncService,
        IngredientSpiritSyncService ingredientSpiritSyncService,
        IngredientSubstitutionSeedService substitutionSeedService)
    {
        _syncService = syncService;
        _flavorTagSyncService = flavorTagSyncService;
        _cocktailFlavorTagSyncService = cocktailFlavorTagSyncService;
        _ingredientSpiritSyncService = ingredientSpiritSyncService;
        _substitutionSeedService = substitutionSeedService;
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

    [HttpPost("tag-cocktail-flavors")]
    public async Task<IActionResult> TagCocktailFlavors()
    {
        var tagsAdded = await _cocktailFlavorTagSyncService.TagAllCocktailsAsync();
        return Ok(new { tagsAdded });
    }

    [HttpPost("identify-ingredient-spirits")]
    public async Task<IActionResult> IdentifyIngredientSpirits()
    {
        var ingredientsMatched = await _ingredientSpiritSyncService.IdentifyAllIngredientSpiritsAsync();
        return Ok(new { ingredientsMatched });
    }

    [HttpPost("seed-ingredient-substitutions")]
    public async Task<IActionResult> SeedIngredientSubstitutions()
    {
        var rulesAdded = await _substitutionSeedService.SeedSubstitutionsAsync();
        return Ok(new { rulesAdded });
    }
}
