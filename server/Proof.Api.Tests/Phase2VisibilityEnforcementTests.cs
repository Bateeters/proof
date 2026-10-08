using Proof.Api.Data;
using Proof.Api.Models;
using Proof.Api.Services;
using Xunit;
using static Proof.Api.Tests.TestDataHelpers;

namespace Proof.Api.Tests;

// Covers the 7 call sites CocktailVisibility.VisibleTo had to be threaded
// into (see DATA_MODEL.md "Custom cocktails"). GetCocktailById/GetCategories/
// Browse are thin controller wrappers around the same predicate already
// proven in CocktailVisibilityTests -- these cover the three services with
// real logic sitting on top of it.
public class Phase2VisibilityEnforcementTests : IDisposable
{
    private readonly SqliteDbContextFactory _factory;
    private readonly ProofDbContext _context;

    public Phase2VisibilityEnforcementTests()
    {
        _factory = new SqliteDbContextFactory();
        _context = _factory.Context;
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task TasteRankingService_NeverRanksAPrivateCocktailForANonOwner()
    {
        var (ownerAccount, ownerProfile) = CreateAccountWithProfile(_context, "owner@test.com");
        var sibling = CreateSiblingProfile(_context, ownerAccount);
        var privateCocktail = CreateCocktail(_context, name: "Secret Mix", isCustom: true, visibility: Visibility.Private, owner: ownerProfile);
        var localCocktail = CreateCocktail(_context, name: "Family Punch", isCustom: true, visibility: Visibility.Local, owner: ownerProfile);
        await _context.SaveChangesAsync();

        var service = new TasteRankingService(_context);
        var resultsForSibling = await service.RankCocktailsForProfileAsync(ownerAccount.Id, sibling.Id);

        Assert.DoesNotContain(resultsForSibling, c => c.Id == privateCocktail.Id);
        Assert.Contains(resultsForSibling, c => c.Id == localCocktail.Id);
    }

    [Fact]
    public async Task TasteRankingService_NeverRanksALocalCocktailForADifferentAccount()
    {
        var (ownerAccount, ownerProfile) = CreateAccountWithProfile(_context, "owner@test.com");
        var (strangerAccount, strangerProfile) = CreateAccountWithProfile(_context, "stranger@test.com");
        var localCocktail = CreateCocktail(_context, name: "Family Punch", isCustom: true, visibility: Visibility.Local, owner: ownerProfile);
        await _context.SaveChangesAsync();

        var service = new TasteRankingService(_context);
        var resultsForStranger = await service.RankCocktailsForProfileAsync(strangerAccount.Id, strangerProfile.Id);

        Assert.DoesNotContain(resultsForStranger, c => c.Id == localCocktail.Id);
    }

    [Fact]
    public async Task CocktailSimilarityService_ExcludesInvisibleCandidates()
    {
        var (ownerAccount, ownerProfile) = CreateAccountWithProfile(_context, "owner@test.com");
        var (strangerAccount, strangerProfile) = CreateAccountWithProfile(_context, "stranger@test.com");
        var target = CreateCocktail(_context, name: "Public Target", category: "Shot", isCustom: false);
        var invisibleSameCategory = CreateCocktail(_context, name: "Hidden Shot", category: "Shot", isCustom: true, visibility: Visibility.Private, owner: ownerProfile);
        await _context.SaveChangesAsync();

        var service = new CocktailSimilarityService(_context);
        var similar = await service.GetSimilarCocktailsAsync(strangerAccount.Id, strangerProfile.Id, target.Id);

        Assert.NotNull(similar);
        Assert.DoesNotContain(similar!, c => c.Id == invisibleSameCategory.Id);
    }

    [Fact]
    public async Task CocktailSimilarityService_ReturnsNullWhenTargetItselfIsInvisible()
    {
        var (ownerAccount, ownerProfile) = CreateAccountWithProfile(_context, "owner@test.com");
        var (strangerAccount, strangerProfile) = CreateAccountWithProfile(_context, "stranger@test.com");
        var privateTarget = CreateCocktail(_context, isCustom: true, visibility: Visibility.Private, owner: ownerProfile);
        await _context.SaveChangesAsync();

        var service = new CocktailSimilarityService(_context);
        var similar = await service.GetSimilarCocktailsAsync(strangerAccount.Id, strangerProfile.Id, privateTarget.Id);

        Assert.Null(similar);
    }

    [Fact]
    public async Task WhatCanIMakeService_NeverSurfacesAnInvisibleCustomCocktail()
    {
        var (ownerAccount, ownerProfile) = CreateAccountWithProfile(_context, "owner@test.com");
        var (strangerAccount, strangerProfile) = CreateAccountWithProfile(_context, "stranger@test.com");

        var lime = new Ingredient { Name = "Lime", Type = IngredientType.Garnish, CostTier = CostTier.Budget, AvailabilityTier = AvailabilityTier.Common };
        _context.Ingredients.Add(lime);

        var privateCocktail = CreateCocktail(_context, name: "Secret Shot", isCustom: true, visibility: Visibility.Private, owner: ownerProfile);
        _context.CocktailIngredients.Add(new CocktailIngredient { Cocktail = privateCocktail, CocktailId = privateCocktail.Id, Ingredient = lime, IngredientId = lime.Id, SortOrder = 0 });
        await _context.SaveChangesAsync();

        var service = new WhatCanIMakeService(_context);
        var results = await service.FindMakeableCocktailsAsync(strangerAccount.Id, strangerProfile.Id, ["lime"]);

        Assert.DoesNotContain(results, c => c.Id == privateCocktail.Id);
    }
}
