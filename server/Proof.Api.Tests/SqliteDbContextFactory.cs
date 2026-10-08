using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;

namespace Proof.Api.Tests;

/// <summary>
/// SQLite in-memory over the EF InMemory provider: this feature's
/// correctness depends on real relational behavior (FK enforcement,
/// EF-to-SQL translation of CocktailVisibility.VisibleTo) that the
/// InMemory provider doesn't faithfully emulate.
/// </summary>
public sealed class SqliteDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public ProofDbContext Context { get; }

    public SqliteDbContextFactory()
    {
        // Connection must stay open for the context's lifetime -- SQLite's
        // :memory: database is destroyed the moment the last connection
        // to it closes.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ProofDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new ProofDbContext(options);

        // EnsureCreated (not the Postgres migrations) -- the migrations
        // contain Postgres-specific DDL (uuid, timestamp with time zone);
        // EnsureCreated builds schema straight from the current EF model,
        // which is provider-agnostic.
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
