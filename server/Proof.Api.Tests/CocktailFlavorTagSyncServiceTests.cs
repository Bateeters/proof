using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.Models;
using Proof.Api.Services;
using Xunit;
using static Proof.Api.Tests.TestDataHelpers;

namespace Proof.Api.Tests;

public class CocktailFlavorTagSyncServiceTests : IDisposable
{
    private readonly SqliteDbContextFactory _factory;
    private readonly ProofDbContext _context;

    public CocktailFlavorTagSyncServiceTests()
    {
        _factory = new SqliteDbContextFactory();
        _context = _factory.Context;
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task TagAllCocktailsAsync_LeavesACustomCocktailsManuallyChosenTagsUntouched()
    {
        var citrus = new FlavorTag { Name = "Citrus" };
        var herbal = new FlavorTag { Name = "Herbal" };
        _context.FlavorTags.AddRange(citrus, herbal);

        var lime = new Ingredient { Name = "Lime", Type = IngredientType.Garnish, CostTier = CostTier.Budget, AvailabilityTier = AvailabilityTier.Common };
        _context.Ingredients.Add(lime);
        _context.IngredientFlavorTags.Add(new IngredientFlavorTag { Ingredient = lime, IngredientId = lime.Id, FlavorTagId = citrus.Id });

        // Non-custom cocktail using Lime -- should get re-tagged Citrus by the bulk job.
        var syncedCocktail = CreateCocktail(_context, name: "Synced Drink", isCustom: false);
        _context.CocktailIngredients.Add(new CocktailIngredient { Cocktail = syncedCocktail, CocktailId = syncedCocktail.Id, Ingredient = lime, IngredientId = lime.Id, SortOrder = 0 });

        // Custom cocktail the user manually tagged Herbal (doesn't actually
        // use Lime -- the point is the bulk job should never touch it).
        var (_, owner) = CreateAccountWithProfile(_context, "owner@test.com");
        var customCocktail = CreateCocktail(_context, name: "Custom Drink", isCustom: true, owner: owner);
        _context.CocktailFlavorTags.Add(new CocktailFlavorTag { Cocktail = customCocktail, CocktailId = customCocktail.Id, FlavorTagId = herbal.Id });

        await _context.SaveChangesAsync();

        var service = new CocktailFlavorTagSyncService(_context);
        await service.TagAllCocktailsAsync();

        var syncedTags = await _context.CocktailFlavorTags.Where(cft => cft.CocktailId == syncedCocktail.Id).Select(cft => cft.FlavorTagId).ToListAsync();
        var customTags = await _context.CocktailFlavorTags.Where(cft => cft.CocktailId == customCocktail.Id).Select(cft => cft.FlavorTagId).ToListAsync();

        Assert.Contains(citrus.Id, syncedTags);
        Assert.Equal([herbal.Id], customTags);
    }
}
