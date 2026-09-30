using Microsoft.EntityFrameworkCore;
using Proof.Api.Models;

namespace Proof.Api.Data;

public class ProofDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Cocktail> Cocktails { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<CocktailIngredient> CocktailIngredients { get; set; }
    public DbSet<CocktailSeason> CocktailSeasons { get; set; }
    public DbSet<Spirit> Spirits { get; set; }
    public DbSet<FlavorTag> FlavorTags { get; set; }
    public DbSet<ProfileSpiritPreference> ProfileSpiritPreferences { get; set; }
    public DbSet<ProfileFlavorPreference> ProfileFlavorPreferences { get; set; }
    public DbSet<ProfileAllergen> ProfileAllergens { get; set; }
    public DbSet<IngredientFlavorTag> IngredientFlavorTags { get; set; }
    public DbSet<CocktailFlavorTag> CocktailFlavorTags { get; set; }
    public DbSet<CookbookEntry> CookbookEntries { get; set; }
    public DbSet<IngredientSubstitution> IngredientSubstitutions { get; set; }
    public ProofDbContext(DbContextOptions<ProofDbContext> options) : base(options) {}
}