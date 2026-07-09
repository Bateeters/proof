using Microsoft.EntityFrameworkCore;

class ProofDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public ProofDbContext(DbContextOptions<ProofDbContext> options) : base(options) {}
}