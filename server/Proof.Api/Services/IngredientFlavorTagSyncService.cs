using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.Models;

namespace Proof.Api.Services;

public class IngredientFlavorTagSyncService
{
    private readonly ProofDbContext _context;

    public IngredientFlavorTagSyncService(ProofDbContext context)
    {
        _context = context;
    }

    public async Task<int> TagAllIngredientsAsync()
    {
        // TODO:
        // 1
        var flavorTagsByName = await _context.FlavorTags
            .ToDictionaryAsync(f => f.Name, f => f.Id);

        // 2
        var ingredients = await _context.Ingredients.ToListAsync();

        // 3
        var existingTags = await _context.IngredientFlavorTags.ToListAsync();
        _context.IngredientFlavorTags.RemoveRange(existingTags);

        // 4 & 5
        var ingredientTagsAdded = 0;

        foreach (var ingredient in ingredients)
        {
            var flavorTagNames = IngredientFlavorHeuristic.AssignFlavorTag(ingredient.Name);

            foreach (var tagName in flavorTagNames)
            {
                var flavorTagId = flavorTagsByName[tagName];
                
                _context.IngredientFlavorTags.Add(new IngredientFlavorTag
                {
                    IngredientId = ingredient.Id,
                    FlavorTagId = flavorTagId
                });

                ingredientTagsAdded++;
            }
        }

        await _context.SaveChangesAsync();
        return ingredientTagsAdded;
    }
}
