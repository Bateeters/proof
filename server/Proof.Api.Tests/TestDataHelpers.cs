using Proof.Api.Data;
using Proof.Api.Models;

namespace Proof.Api.Tests;

public static class TestDataHelpers
{
    public static (Account account, Profile profile) CreateAccountWithProfile(ProofDbContext context, string email)
    {
        var account = new Account { Email = email, PasswordHash = "hash" };
        var profile = new Profile { AccountId = account.Id, Account = account, DisplayName = "P", AvatarColor = "gray" };
        context.Accounts.Add(account);
        context.Profiles.Add(profile);
        return (account, profile);
    }

    public static Profile CreateSiblingProfile(ProofDbContext context, Account account, string displayName = "Sibling")
    {
        var profile = new Profile { AccountId = account.Id, Account = account, DisplayName = displayName, AvatarColor = "blue" };
        context.Profiles.Add(profile);
        return profile;
    }

    public static Cocktail CreateCocktail(
        ProofDbContext context,
        string name = "Test Cocktail",
        string category = "Cocktail",
        bool isCustom = false,
        Visibility visibility = Visibility.Private,
        Profile? owner = null,
        bool isDeleted = false)
    {
        var cocktail = new Cocktail
        {
            Name = name,
            Category = category,
            Glass = "Coupe",
            Instructions = "Shake and strain.",
            IsCustom = isCustom,
            Visibility = visibility,
            IsDeleted = isDeleted,
            OwnerProfileId = owner?.Id,
            OwnerProfile = owner
        };
        context.Cocktails.Add(cocktail);
        return cocktail;
    }
}
