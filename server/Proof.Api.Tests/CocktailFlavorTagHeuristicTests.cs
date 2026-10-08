using Proof.Api.Services;
using Xunit;

namespace Proof.Api.Tests;

public class CocktailFlavorTagHeuristicTests
{
    [Fact]
    public void ComputeProminentTags_DominantTagWinsOverMinorOne()
    {
        // 3 ingredients tagged "Citrus", 1 tagged "Herbal" -- Citrus is the
        // max (3), Herbal (1) fails 1*2 >= 3, so only Citrus survives.
        var ingredientTagSets = new[]
        {
            new HashSet<string> { "Citrus" },
            new HashSet<string> { "Citrus" },
            new HashSet<string> { "Citrus" },
            new HashSet<string> { "Herbal" }
        };

        var result = CocktailFlavorTagHeuristic.ComputeProminentTags(ingredientTagSets);

        Assert.Contains("Citrus", result);
        Assert.DoesNotContain("Herbal", result);
    }

    [Fact]
    public void ComputeProminentTags_TiedTagsBothSurvive()
    {
        var ingredientTagSets = new[]
        {
            new HashSet<string> { "Sweet" },
            new HashSet<string> { "Sour" }
        };

        var result = CocktailFlavorTagHeuristic.ComputeProminentTags(ingredientTagSets);

        Assert.Contains("Sweet", result);
        Assert.Contains("Sour", result);
    }

    [Fact]
    public void ComputeProminentTags_NoTagsAtAll_ReturnsEmpty()
    {
        var result = CocktailFlavorTagHeuristic.ComputeProminentTags([]);

        Assert.Empty(result);
    }
}
