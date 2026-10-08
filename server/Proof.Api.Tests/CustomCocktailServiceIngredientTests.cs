using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.DTOs;
using Proof.Api.Models;
using Proof.Api.Services;
using Xunit;

namespace Proof.Api.Tests;

public class CustomCocktailServiceIngredientTests : IDisposable
{
    private readonly SqliteDbContextFactory _factory;
    private readonly ProofDbContext _context;
    private readonly CustomCocktailService _service;

    public CustomCocktailServiceIngredientTests()
    {
        _factory = new SqliteDbContextFactory();
        _context = _factory.Context;
        _service = new CustomCocktailService(_context);
    }

    public void Dispose() => _factory.Dispose();

    private async Task<Spirit> SeedSpiritAsync(string name)
    {
        var spirit = new Spirit { Name = name };
        _context.Spirits.Add(spirit);
        await _context.SaveChangesAsync();
        return spirit;
    }

    [Fact]
    public async Task NewIngredient_IsNormalized_TrimmedCollapsedTitleCased()
    {
        var (success, error, ingredient) = await _service.ResolveIngredientAsync(
            new CocktailIngredientInputDto { NewIngredientName = "  fresh    mint leaves  " });

        Assert.True(success, error);
        Assert.Equal("Fresh Mint Leaves", ingredient!.Name);
        Assert.Equal(IngredientType.Other, ingredient.Type);
        Assert.Equal(CostTier.Mid, ingredient.CostTier);
        Assert.Equal(AvailabilityTier.Common, ingredient.AvailabilityTier);
    }

    [Fact]
    public async Task NewIngredient_CaseInsensitiveMatch_ReusesExistingRow_NoDuplicate()
    {
        var existing = new Ingredient { Name = "Dark Rum", Type = IngredientType.Spirit, CostTier = CostTier.Mid, AvailabilityTier = AvailabilityTier.Common };
        _context.Ingredients.Add(existing);
        await _context.SaveChangesAsync();

        var (success, error, resolved) = await _service.ResolveIngredientAsync(
            new CocktailIngredientInputDto { NewIngredientName = "dark rum" });

        Assert.True(success, error);
        Assert.Equal(existing.Id, resolved!.Id);
        Assert.Equal(1, await _context.Ingredients.CountAsync(i => i.Name.ToLower() == "dark rum"));
    }

    [Fact]
    public async Task NewIngredient_HeuristicDetectsSpirit_NotManuallyTagged()
    {
        var rum = await SeedSpiritAsync("Rum");

        var (success, error, ingredient) = await _service.ResolveIngredientAsync(
            new CocktailIngredientInputDto { NewIngredientName = "Spiced Rum" });

        Assert.True(success, error);
        Assert.Equal(rum.Id, ingredient!.SpiritId);
        Assert.False(ingredient.IsManuallyTagged);
    }

    [Fact]
    public async Task NewIngredient_ManualSpiritOverride_SetsIsManuallyTagged()
    {
        var mysterySpirit = await SeedSpiritAsync("Mystery Spirit");

        var (success, error, ingredient) = await _service.ResolveIngredientAsync(
            new CocktailIngredientInputDto { NewIngredientName = "Grandma's Secret Bottle", SpiritId = mysterySpirit.Id });

        Assert.True(success, error);
        Assert.Equal(mysterySpirit.Id, ingredient!.SpiritId);
        Assert.True(ingredient.IsManuallyTagged);
    }

    [Fact]
    public async Task NewIngredient_NoHeuristicMatch_NoSpiritIdSet()
    {
        var (success, error, ingredient) = await _service.ResolveIngredientAsync(
            new CocktailIngredientInputDto { NewIngredientName = "Blueberry Thyme Shrub" });

        Assert.True(success, error);
        Assert.Null(ingredient!.SpiritId);
        Assert.False(ingredient.IsManuallyTagged);
    }

    [Fact]
    public async Task UnknownIngredientId_IsRejected()
    {
        var (success, error, ingredient) = await _service.ResolveIngredientAsync(
            new CocktailIngredientInputDto { IngredientId = Guid.NewGuid() });

        Assert.False(success);
        Assert.NotNull(error);
        Assert.Null(ingredient);
    }

    [Fact]
    public async Task BothIdAndName_IsRejected()
    {
        var existing = new Ingredient { Name = "Lime", Type = IngredientType.Garnish, CostTier = CostTier.Budget, AvailabilityTier = AvailabilityTier.Common };
        _context.Ingredients.Add(existing);
        await _context.SaveChangesAsync();

        var (success, error, _) = await _service.ResolveIngredientAsync(
            new CocktailIngredientInputDto { IngredientId = existing.Id, NewIngredientName = "Lemon" });

        Assert.False(success);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task NeitherIdNorName_IsRejected()
    {
        var (success, error, _) = await _service.ResolveIngredientAsync(new CocktailIngredientInputDto());

        Assert.False(success);
        Assert.NotNull(error);
    }
}
