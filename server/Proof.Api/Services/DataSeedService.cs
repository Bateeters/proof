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
        "Sweet", "Sour", "Bitter", "Citrus", "Herbal", "Spicy",
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
        if (await _context.Spirits.AnyAsync())
        {
            return;
        }

        foreach (var name in SpiritNames)
        {
            _context.Spirits.Add(new Spirit { Name = name });
        }
    }

    private async Task SeedFlavorTagsAsync()
    {
        if (await _context.FlavorTags.AnyAsync())
        {
            return;
        }

        foreach (var name in FlavorTagNames)
        {
            _context.FlavorTags.Add(new FlavorTag { Name = name });
        }
    }
}
