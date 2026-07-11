using Microsoft.EntityFrameworkCore;
using Proof.Api.Models;

namespace Proof.Api.Data;

public class ProofDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public ProofDbContext(DbContextOptions<ProofDbContext> options) : base(options) {}
}