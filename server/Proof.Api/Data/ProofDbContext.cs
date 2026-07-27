using Microsoft.EntityFrameworkCore;
using Proof.Api.Models;

namespace Proof.Api.Data;

public class ProofDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Cocktail> Cocktails { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<CocktailIngredient> CocktailIngredients { get; set; }
    public DbSet<CocktailSeason> CocktailSeasons { get; set; }
    public ProofDbContext(DbContextOptions<ProofDbContext> options) : base(options) {}
}