using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.Models;

namespace Proof.Api.Services;

public class DataSeedService
{
    private readonly ProofDbContext _context;

    private static readonly string[] SpiritNames =
    {
        "Vodka", "Gin", "Rum", "Tequila", "Whiskey", "Bourbon",
        "Scotch", "Brandy", "Cognac", "Mezcal", "Vermouth", "Triple Sec"
    };

    private static readonly string[] FlavorTagNames =
    {
        "Sweet", "Sour", "Bitter", "Citrus", "Herbal", "Spicy", "Spiced",
        "Smoky", "Floral", "Fruity", "Creamy", "Nutty", "Refreshing"
    };

    public DataSeedService(ProofDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await SeedSpiritsAsync();
        await SeedFlavorTagsAsync();
        await _context.SaveChangesAsync();
    }

    private async Task SeedSpiritsAsync()
    {
        // Check which names already exist (one query), then only add the ones
        // that don't — rather than bailing out entirely if the table has
        // *anything* in it. That way, adding a new name to the list above
        // later still gets it seeded on the next startup, instead of being
        // silently skipped just because the table already has older data.
        var existingNames = await _context.Spirits
            .Select(s => s.Name)
            .ToListAsync();

        foreach (var name in SpiritNames)
        {
            if (!existingNames.Contains(name))
            {
                _context.Spirits.Add(new Spirit { Name = name });
            }
        }
    }

    private async Task SeedFlavorTagsAsync()
    {
        var existingNames = await _context.FlavorTags
            .Select(f => f.Name)
            .ToListAsync();

        foreach (var name in FlavorTagNames)
        {
            if (!existingNames.Contains(name))
            {
                _context.FlavorTags.Add(new FlavorTag { Name = name });
            }
        }
    }
}
