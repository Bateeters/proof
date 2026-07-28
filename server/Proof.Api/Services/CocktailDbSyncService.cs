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

        var ingredientNames = drink.GetIngredients().Select(i => i.Name);
        foreach (var season in SeasonHeuristic.AssignSeasons(ingredientNames))
        {
            _context.CocktailSeasons.Add(new CocktailSeason
            {
                CocktailId = cocktail.Id,
                Cocktail = cocktail,
                Season = season
            });
        }

        return true;
    }

    private async Task<Ingredient> GetOrCreateIngredientAsync(string name)
    {
        // Check already-tracked-but-not-yet-saved ingredients first (added earlier
        // in this same sync run), then fall back to the database for ingredients
        // that persisted from a previous sync. Without the .Local check, every
        // ingredient would get re-created once per cocktail that uses it, since
        // SaveChangesAsync only runs once at the very end of the whole sync.
        var existing = _context.Ingredients.Local.FirstOrDefault(i => i.Name == name)
            ?? await _context.Ingredients.FirstOrDefaultAsync(i => i.Name == name);

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
