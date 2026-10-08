using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.DTOs;
using Proof.Api.Models;
using Proof.Api.Services;
using Xunit;
using static Proof.Api.Tests.TestDataHelpers;

namespace Proof.Api.Tests;

public class CustomCocktailServiceSaveTests : IDisposable
{
    private readonly SqliteDbContextFactory _factory;
    private readonly ProofDbContext _context;
    private readonly CustomCocktailService _service;

    public CustomCocktailServiceSaveTests()
    {
        _factory = new SqliteDbContextFactory();
        _context = _factory.Context;
        _service = new CustomCocktailService(_context);
    }

    public void Dispose() => _factory.Dispose();

    private static SaveCustomCocktailDto ValidRequest(Visibility visibility = Visibility.Private) => new()
    {
        Name = "My Mix",
        Category = "Cocktail",
        Glass = "Coupe",
        Instructions = "Shake well.",
        Visibility = visibility,
        Ingredients = [new CocktailIngredientInputDto { NewIngredientName = "Vodka" }]
    };

    // Ownership-scoping, same query shape as
    // ProfilesController.GetOwnedCustomCocktailAsync.
    private async Task<Cocktail?> GetOwnedCustomCocktailAsync(Guid profileId, Guid cocktailId) =>
        await _context.Cocktails.FirstOrDefaultAsync(c => c.Id == cocktailId && c.OwnerProfileId == profileId && c.IsCustom && !c.IsDeleted);

    [Fact]
    public async Task Create_SetsOwnershipAndIsCustom()
    {
        var (_, owner) = CreateAccountWithProfile(_context, "owner@test.com");
        await _context.SaveChangesAsync();

        var (success, error, cocktail) = await _service.CreateAsync(owner.Id, ValidRequest());

        Assert.True(success, error);
        Assert.True(cocktail!.IsCustom);
        Assert.Equal(owner.Id, cocktail.OwnerProfileId);
    }

    [Theory]
    [InlineData("", "Cocktail", "Coupe", "Shake.")]
    [InlineData("Name", "", "Coupe", "Shake.")]
    [InlineData("Name", "Cocktail", "", "Shake.")]
    [InlineData("Name", "Cocktail", "Coupe", "")]
    public async Task Create_RejectsMissingRequiredFields(string name, string category, string glass, string instructions)
    {
        var (_, owner) = CreateAccountWithProfile(_context, "owner@test.com");
        await _context.SaveChangesAsync();

        var request = new SaveCustomCocktailDto
        {
            Name = name,
            Category = category,
            Glass = glass,
            Instructions = instructions,
            Visibility = Visibility.Private,
            Ingredients = [new CocktailIngredientInputDto { NewIngredientName = "Vodka" }]
        };

        var (success, error, _) = await _service.CreateAsync(owner.Id, request);

        Assert.False(success);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task Create_RejectsZeroIngredients()
    {
        var (_, owner) = CreateAccountWithProfile(_context, "owner@test.com");
        await _context.SaveChangesAsync();

        var request = ValidRequest();
        request.Ingredients = [];

        var (success, error, _) = await _service.CreateAsync(owner.Id, request);

        Assert.False(success);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task Create_RejectsUnknownFlavorTagId()
    {
        var (_, owner) = CreateAccountWithProfile(_context, "owner@test.com");
        await _context.SaveChangesAsync();

        var request = ValidRequest();
        request.FlavorTagIds = [Guid.NewGuid()];

        var (success, error, _) = await _service.CreateAsync(owner.Id, request);

        Assert.False(success);
        Assert.NotNull(error);
        // Nothing partially persisted -- the cocktail never got saved either.
        Assert.Equal(0, await _context.Cocktails.CountAsync());
    }

    [Fact]
    public async Task Update_FullyReplacesIngredientsFlavorTagsAndSeasons()
    {
        var (_, owner) = CreateAccountWithProfile(_context, "owner@test.com");
        var citrus = new FlavorTag { Name = "Citrus" };
        var herbal = new FlavorTag { Name = "Herbal" };
        _context.FlavorTags.AddRange(citrus, herbal);
        await _context.SaveChangesAsync();

        var createRequest = ValidRequest();
        createRequest.FlavorTagIds = [citrus.Id];
        createRequest.Seasons = [Season.Summer];
        var (createSuccess, createError, cocktail) = await _service.CreateAsync(owner.Id, createRequest);
        Assert.True(createSuccess, createError);

        var updateRequest = new SaveCustomCocktailDto
        {
            Name = "Renamed",
            Category = "Cocktail",
            Glass = "Rocks",
            Instructions = "Stir instead.",
            Visibility = Visibility.Global,
            Ingredients = [new CocktailIngredientInputDto { NewIngredientName = "Gin" }],
            FlavorTagIds = [herbal.Id],
            Seasons = [Season.Winter]
        };
        var (updateSuccess, updateError) = await _service.UpdateAsync(cocktail!, updateRequest);
        Assert.True(updateSuccess, updateError);

        var reloaded = await _context.Cocktails
            .Include(c => c.CocktailIngredients).ThenInclude(ci => ci.Ingredient)
            .Include(c => c.CocktailFlavorTags)
            .Include(c => c.CocktailSeasons)
            .FirstAsync(c => c.Id == cocktail!.Id);

        Assert.Equal("Renamed", reloaded.Name);
        Assert.Equal(Visibility.Global, reloaded.Visibility);
        Assert.Single(reloaded.CocktailIngredients);
        Assert.Equal("Gin", reloaded.CocktailIngredients.First().Ingredient.Name);
        Assert.Equal([herbal.Id], reloaded.CocktailFlavorTags.Select(cft => cft.FlavorTagId));
        Assert.Equal([Season.Winter], reloaded.CocktailSeasons.Select(cs => cs.Season));
    }

    [Fact]
    public async Task OwnershipScoping_BlocksNonOwner()
    {
        var (ownerAccount, owner) = CreateAccountWithProfile(_context, "owner@test.com");
        var sibling = CreateSiblingProfile(_context, ownerAccount);
        var cocktail = CreateCocktail(_context, isCustom: true, owner: owner);
        await _context.SaveChangesAsync();

        Assert.Null(await GetOwnedCustomCocktailAsync(sibling.Id, cocktail.Id));
        Assert.NotNull(await GetOwnedCustomCocktailAsync(owner.Id, cocktail.Id));
    }

    [Fact]
    public async Task OwnershipScoping_BlocksNonCustomCocktail()
    {
        var (_, owner) = CreateAccountWithProfile(_context, "owner@test.com");
        // Pathological case: a synced catalog cocktail whose OwnerProfileId
        // somehow matches -- must still be blocked since IsCustom is false.
        var cocktail = CreateCocktail(_context, isCustom: false, owner: owner);
        await _context.SaveChangesAsync();

        Assert.Null(await GetOwnedCustomCocktailAsync(owner.Id, cocktail.Id));
    }

    [Fact]
    public async Task OwnershipScoping_BlocksAlreadyDeletedCocktail()
    {
        var (_, owner) = CreateAccountWithProfile(_context, "owner@test.com");
        var cocktail = CreateCocktail(_context, isCustom: true, owner: owner, isDeleted: true);
        await _context.SaveChangesAsync();

        Assert.Null(await GetOwnedCustomCocktailAsync(owner.Id, cocktail.Id));
    }

    [Fact]
    public async Task Delete_SetsIsDeleted_WithoutRemovingChildRowsOrOthersCookbookEntries()
    {
        var (_, owner) = CreateAccountWithProfile(_context, "owner@test.com");
        var (_, saver) = CreateAccountWithProfile(_context, "saver@test.com");
        var cocktail = CreateCocktail(_context, isCustom: true, visibility: Visibility.Global, owner: owner);
        var vodka = new Ingredient { Name = "Vodka", Type = IngredientType.Spirit, CostTier = CostTier.Mid, AvailabilityTier = AvailabilityTier.Common };
        _context.Ingredients.Add(vodka);
        _context.CocktailIngredients.Add(new CocktailIngredient { Cocktail = cocktail, CocktailId = cocktail.Id, Ingredient = vodka, IngredientId = vodka.Id, SortOrder = 0 });
        _context.CookbookEntries.Add(new CookbookEntry { ProfileId = saver.Id, CocktailId = cocktail.Id });
        await _context.SaveChangesAsync();

        // Same effect as ProfilesController.DeleteCocktail -- no cascade.
        cocktail.IsDeleted = true;
        await _context.SaveChangesAsync();

        Assert.True((await _context.Cocktails.FindAsync(cocktail.Id))!.IsDeleted);
        Assert.Equal(1, await _context.CocktailIngredients.CountAsync(ci => ci.CocktailId == cocktail.Id));
        Assert.Equal(1, await _context.CookbookEntries.CountAsync(ce => ce.CocktailId == cocktail.Id));
    }

    [Fact]
    public async Task GetMyCocktails_ReturnsAllNonDeletedTiersForOwner_NothingForAnyoneElse()
    {
        var (ownerAccount, owner) = CreateAccountWithProfile(_context, "owner@test.com");
        var sibling = CreateSiblingProfile(_context, ownerAccount);
        var privateCocktail = CreateCocktail(_context, name: "Private One", isCustom: true, visibility: Visibility.Private, owner: owner);
        var globalCocktail = CreateCocktail(_context, name: "Global One", isCustom: true, visibility: Visibility.Global, owner: owner);
        var deletedCocktail = CreateCocktail(_context, name: "Deleted One", isCustom: true, owner: owner, isDeleted: true);
        await _context.SaveChangesAsync();

        var ownerView = await _context.Cocktails.Where(c => c.OwnerProfileId == owner.Id && !c.IsDeleted).Select(c => c.Id).ToListAsync();
        var siblingView = await _context.Cocktails.Where(c => c.OwnerProfileId == sibling.Id && !c.IsDeleted).Select(c => c.Id).ToListAsync();

        Assert.Contains(privateCocktail.Id, ownerView);
        Assert.Contains(globalCocktail.Id, ownerView);
        Assert.DoesNotContain(deletedCocktail.Id, ownerView);
        Assert.Empty(siblingView);
    }
}
