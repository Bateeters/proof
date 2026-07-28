using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.Models;

namespace Proof.Api.Services;

public class CocktailDbSyncService
{
    private readonly HttpClient _httpClient;
    private readonly ProofDbContext _context;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CocktailDbSyncService(HttpClient httpClient, ProofDbContext context)
    {
        _httpClient = httpClient;
        _context = context;
    }

    public async Task<int> SyncAllCocktailsAsync()
    {
        var cocktailsAdded = 0;

        foreach (var letter in "abcdefghijklmnopqrstuvwxyz")
        {
            var response = await _httpClient.GetFromJsonAsync<CocktailDbSearchResponse>(
                $"search.php?f={letter}", JsonOptions);

            if (response?.Drinks == null)
            {
                continue;
            }

            foreach (var drink in response.Drinks)
            {
                var wasAdded = await AddCocktailIfNewAsync(drink);
                if (wasAdded)
                {
                    cocktailsAdded++;
                }
            }
        }

        await _context.SaveChangesAsync();
        return cocktailsAdded;
    }

    private async Task<bool> AddCocktailIfNewAsync(CocktailDbDrink drink)
    {
        var alreadyExists = await _context.Cocktails
            .AnyAsync(c => c.ExternalId == drink.IdDrink);

        if (alreadyExists)
        {
            return false;
        }

        var cocktail = new Cocktail
        {
            ExternalId = drink.IdDrink,
            Name = drink.StrDrink,
            Category = drink.StrCategory ?? "Uncategorized",
            Glass = drink.StrGlass ?? "Unspecified",
            Instructions = drink.StrInstructions ?? "",
            ImageUrl = drink.StrDrinkThumb
        };

        _context.Cocktails.Add(cocktail);

        var sortOrder = 0;
        foreach (var (ingredientName, measure) in drink.GetIngredients())
        {
            var ingredient = await GetOrCreateIngredientAsync(ingredientName);

            _context.CocktailIngredients.Add(new CocktailIngredient
            {
                CocktailId = cocktail.Id,
                Cocktail = cocktail,
                IngredientId = ingredient.Id,
                Ingredient = ingredient,
                Measure = measure,
                SortOrder = sortOrder++
            });
        }

        return true;
    }

    private async Task<Ingredient> GetOrCreateIngredientAsync(string name)
    {
        var existing = await _context.Ingredients
            .FirstOrDefaultAsync(i => i.Name == name);

        if (existing != null)
        {
            return existing;
        }

        var ingredient = new Ingredient
        {
            Name = name,
            Type = IngredientType.Other,
            CostTier = CostTier.Mid,
            AvailabilityTier = AvailabilityTier.Common
        };

        _context.Ingredients.Add(ingredient);
        return ingredient;
    }
}
