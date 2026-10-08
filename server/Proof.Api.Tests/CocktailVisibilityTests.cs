using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.Models;
using Proof.Api.Services;
using Xunit;
using static Proof.Api.Tests.TestDataHelpers;

namespace Proof.Api.Tests;

public class CocktailVisibilityTests : IDisposable
{
    private readonly SqliteDbContextFactory _factory;
    private readonly ProofDbContext _context;

    public CocktailVisibilityTests()
    {
        _factory = new SqliteDbContextFactory();
        _context = _factory.Context;
    }

    public void Dispose() => _factory.Dispose();

    private async Task<bool> IsVisibleAsync(Cocktail cocktail, Guid callerAccountId, Guid? callerProfileId)
    {
        await _context.SaveChangesAsync();
        var saved = await CocktailVisibility.GetSavedCocktailIdsAsync(_context, callerProfileId);
        return await _context.Cocktails
            .Where(CocktailVisibility.VisibleTo(callerAccountId, callerProfileId, saved))
            .AnyAsync(c => c.Id == cocktail.Id);
    }

    [Fact]
    public async Task SyncedCocktail_IsVisibleToAnyone()
    {
        var (strangerAccount, _) = CreateAccountWithProfile(_context, "stranger@test.com");
        var cocktail = CreateCocktail(_context, isCustom: false);

        Assert.True(await IsVisibleAsync(cocktail, strangerAccount.Id, null));
    }

    [Fact]
    public async Task GlobalCocktail_IsVisibleToUnrelatedAccount()
    {
        var (ownerAccount, ownerProfile) = CreateAccountWithProfile(_context, "owner@test.com");
        var (strangerAccount, _) = CreateAccountWithProfile(_context, "stranger@test.com");
        var cocktail = CreateCocktail(_context, isCustom: true, visibility: Visibility.Global, owner: ownerProfile);

        Assert.True(await IsVisibleAsync(cocktail, strangerAccount.Id, null));
    }

    [Fact]
    public async Task LocalCocktail_IsVisibleToSiblingProfile_ButNotDifferentAccount()
    {
        var (ownerAccount, ownerProfile) = CreateAccountWithProfile(_context, "owner@test.com");
        var siblingProfile = CreateSiblingProfile(_context, ownerAccount);
        var (strangerAccount, _) = CreateAccountWithProfile(_context, "stranger@test.com");
        var cocktail = CreateCocktail(_context, isCustom: true, visibility: Visibility.Local, owner: ownerProfile);

        Assert.True(await IsVisibleAsync(cocktail, ownerAccount.Id, siblingProfile.Id));
        Assert.False(await IsVisibleAsync(cocktail, strangerAccount.Id, null));
    }

    [Fact]
    public async Task PrivateCocktail_IsVisibleOnlyToExactOwner()
    {
        var (ownerAccount, ownerProfile) = CreateAccountWithProfile(_context, "owner@test.com");
        var siblingProfile = CreateSiblingProfile(_context, ownerAccount);
        var cocktail = CreateCocktail(_context, isCustom: true, visibility: Visibility.Private, owner: ownerProfile);

        Assert.True(await IsVisibleAsync(cocktail, ownerAccount.Id, ownerProfile.Id));
        Assert.False(await IsVisibleAsync(cocktail, ownerAccount.Id, siblingProfile.Id));
        Assert.False(await IsVisibleAsync(cocktail, ownerAccount.Id, null));
    }

    [Fact]
    public async Task DeletedCocktail_IsInvisibleToSomeoneWhoNeverSavedIt()
    {
        var (ownerAccount, ownerProfile) = CreateAccountWithProfile(_context, "owner@test.com");
        var (otherAccount, otherProfile) = CreateAccountWithProfile(_context, "other@test.com");
        var cocktail = CreateCocktail(_context, isCustom: true, visibility: Visibility.Global, owner: ownerProfile, isDeleted: true);

        Assert.False(await IsVisibleAsync(cocktail, otherAccount.Id, otherProfile.Id));
    }

    [Fact]
    public async Task DeletedCocktail_IsStillVisibleToAProfileThatAlreadySavedIt()
    {
        var (ownerAccount, ownerProfile) = CreateAccountWithProfile(_context, "owner@test.com");
        var (saverAccount, saverProfile) = CreateAccountWithProfile(_context, "saver@test.com");
        var cocktail = CreateCocktail(_context, isCustom: true, visibility: Visibility.Global, owner: ownerProfile, isDeleted: true);
        _context.CookbookEntries.Add(new CookbookEntry { ProfileId = saverProfile.Id, CocktailId = cocktail.Id });

        Assert.True(await IsVisibleAsync(cocktail, saverAccount.Id, saverProfile.Id));
    }

    [Fact]
    public async Task NarrowedToPrivate_IsStillVisibleToAProfileThatAlreadySavedIt_ButNotAStranger()
    {
        var (ownerAccount, ownerProfile) = CreateAccountWithProfile(_context, "owner@test.com");
        var (saverAccount, saverProfile) = CreateAccountWithProfile(_context, "saver@test.com");
        var (strangerAccount, strangerProfile) = CreateAccountWithProfile(_context, "stranger@test.com");
        // Was Global when saverProfile bookmarked it, now narrowed to Private.
        var cocktail = CreateCocktail(_context, isCustom: true, visibility: Visibility.Private, owner: ownerProfile);
        _context.CookbookEntries.Add(new CookbookEntry { ProfileId = saverProfile.Id, CocktailId = cocktail.Id });

        Assert.True(await IsVisibleAsync(cocktail, saverAccount.Id, saverProfile.Id));
        Assert.False(await IsVisibleAsync(cocktail, strangerAccount.Id, strangerProfile.Id));
    }
}
