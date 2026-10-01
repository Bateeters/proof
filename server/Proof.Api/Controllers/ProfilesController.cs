using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.DTOs;
using Proof.Api.Models;
using Proof.Api.Services;

namespace Proof.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfilesController : ControllerBase
{
    private readonly ProofDbContext _context;
    private readonly TasteRankingService _tasteRankingService;

    public ProfilesController(ProofDbContext context, TasteRankingService tasteRankingService)
    {
        _context = context;
        _tasteRankingService = tasteRankingService;
    }

    private Guid GetAccountId()
    {
        var accountIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value;
        return Guid.Parse(accountIdClaim);
    }

    private async Task<Profile?> GetOwnedProfileAsync(Guid profileId)
    {
        var accountId = GetAccountId();
        return await _context.Profiles
            .FirstOrDefaultAsync(p => p.Id == profileId && p.AccountId == accountId);
    }

    [HttpGet]
    public async Task<IActionResult> GetProfiles()
    {
        var accountId = GetAccountId();

        var profiles = await _context.Profiles
            .Where(p => p.AccountId == accountId)
            .ToListAsync();

        var profileDto = profiles.Select(profile => new ProfileDto
        {
            Id = profile.Id,
            DisplayName = profile.DisplayName,
            AvatarColor = profile.AvatarColor,
            CreatedAt = profile.CreatedAt
        });

        return Ok(profileDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProfile(CreateProfileDto request)
    {
        var accountId = GetAccountId();

        var newProfile = new Profile
        {
            AccountId = accountId,
            DisplayName = request.DisplayName,
            AvatarColor = request.AvatarColor
        };

        _context.Profiles.Add(newProfile);
        await _context.SaveChangesAsync();

        return Ok(new ProfileDto
        {
            Id = newProfile.Id,
            DisplayName = newProfile.DisplayName,
            AvatarColor = newProfile.AvatarColor,
            CreatedAt = newProfile.CreatedAt
        });
    }

    [HttpGet("{id}/preferences")]
    public async Task<IActionResult> GetPreferences(Guid id)
    {
        var profile = await GetOwnedProfileAsync(id);
        if (profile == null)
        {
            return NotFound();
        }

        var spiritPreferences = await _context.ProfileSpiritPreferences
            .Where(sp => sp.ProfileId == id)
            .Select(sp => new SpiritPreferenceDto
            {
                SpiritId = sp.SpiritId,
                SpiritName = sp.Spirit.Name,
                Sentiment = sp.Sentiment
            })
            .ToListAsync();

        var flavorPreferences = await _context.ProfileFlavorPreferences
            .Where(fp => fp.ProfileId == id)
            .Select(fp => new FlavorPreferenceDto
            {
                FlavorTagId = fp.FlavorTagId,
                FlavorTagName = fp.FlavorTag.Name,
                Sentiment = fp.Sentiment
            })
            .ToListAsync();

        var allergens = await _context.ProfileAllergens
            .Where(a => a.ProfileId == id)
            .Select(a => a.Name)
            .ToListAsync();

        var profilePreferencesDto = new ProfilePreferencesDto
        {
            SpiritPreferences = spiritPreferences,
            FlavorPreferences = flavorPreferences,
            Allergens = allergens
        };

        return Ok(profilePreferencesDto);
    }

    [HttpPut("{id}/preferences")]
    public async Task<IActionResult> UpdatePreferences(Guid id, UpdateProfilePreferencesDto request)
    {
        var profile = await GetOwnedProfileAsync(id);
        if (profile == null)
        {
            return NotFound();
        }

        var existingSpiritPreferences = await _context.ProfileSpiritPreferences
            .Where(sp => sp.ProfileId == id)
            .ToListAsync();
        _context.ProfileSpiritPreferences.RemoveRange(existingSpiritPreferences);

        var existingFlavorPreferences = await _context.ProfileFlavorPreferences
            .Where(fp => fp.ProfileId == id)
            .ToListAsync();
        _context.ProfileFlavorPreferences.RemoveRange(existingFlavorPreferences);

        var existingAllergens = await _context.ProfileAllergens
            .Where(a => a.ProfileId == id)
            .ToListAsync();
        _context.ProfileAllergens.RemoveRange(existingAllergens);

        foreach (SpiritPreferenceInputDto sp in request.SpiritPreferences)
        {
            var newSpiritPreference = new ProfileSpiritPreference
            {
                ProfileId = id,
                SpiritId = sp.SpiritId,
                Sentiment = sp.Sentiment
            };

            _context.ProfileSpiritPreferences.Add(newSpiritPreference);
        }

        foreach (FlavorPreferenceInputDto fp in request.FlavorPreferences)
        {
            var newFlavorPreference = new ProfileFlavorPreference
            {
                ProfileId = id,
                FlavorTagId = fp.FlavorTagId,
                Sentiment = fp.Sentiment
            };

            _context.ProfileFlavorPreferences.Add(newFlavorPreference);
        }

        foreach ( string a in request.Allergens)
        {
            var newAllergen = new ProfileAllergen
            {                
                ProfileId = id,
                Name = a
            };

            _context.ProfileAllergens.Add(newAllergen);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/recommendations")]
    public async Task<IActionResult> GetRecommendations(Guid id)
    {
        var profile = await GetOwnedProfileAsync(id);
        if (profile == null)
        {
            return NotFound();
        }

        var ranked = await _tasteRankingService.RankCocktailsForProfileAsync(id);
        return Ok(ranked);
    }

    [HttpGet("{id}/cookbook")]
    public async Task<IActionResult> GetCookbook(Guid id)
    {
        var profile = await GetOwnedProfileAsync(id);
        if (profile == null)
        {
            return NotFound();
        }

        var entries = await _context.CookbookEntries
            .Where(e => e.ProfileId == id)
            .OrderByDescending(e => e.SavedAt)
            .Select(e => new CookbookEntryDto
            {
                CocktailId = e.CocktailId,
                CocktailName = e.Cocktail.Name,
                CocktailCategory = e.Cocktail.Category,
                CocktailImageUrl = e.Cocktail.ImageUrl,
                FlavorTags = e.Cocktail.CocktailFlavorTags.Select(cft => cft.FlavorTag.Name).ToList(),
                SavedAt = e.SavedAt,
                Notes = e.Notes
            })
            .ToListAsync();

        return Ok(entries);
    }

    [HttpPost("{id}/cookbook")]
    public async Task<IActionResult> SaveCookbookEntry(Guid id, SaveCookbookEntryDto request)
    {
        var profile = await GetOwnedProfileAsync(id);
        if (profile == null)
        {
            return NotFound();
        }

        var cocktailExists = await _context.Cocktails.AnyAsync(c => c.Id == request.CocktailId);
        if (!cocktailExists)
        {
            return NotFound();
        }

        var existingEntry = await _context.CookbookEntries
            .FirstOrDefaultAsync(e => e.ProfileId == id && e.CocktailId == request.CocktailId);

        // Saving a cocktail that's already in the cookbook updates the notes
        // instead of creating a duplicate row — re-saving to change a note is
        // more useful than erroring or silently duplicating the entry.
        if (existingEntry != null)
        {
            existingEntry.Notes = request.Notes;
        }
        else
        {
            _context.CookbookEntries.Add(new CookbookEntry
            {
                ProfileId = id,
                CocktailId = request.CocktailId,
                Notes = request.Notes
            });
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}/cookbook/{cocktailId}")]
    public async Task<IActionResult> RemoveCookbookEntry(Guid id, Guid cocktailId)
    {
        var profile = await GetOwnedProfileAsync(id);
        if (profile == null)
        {
            return NotFound();
        }

        var entry = await _context.CookbookEntries
            .FirstOrDefaultAsync(e => e.ProfileId == id && e.CocktailId == cocktailId);

        if (entry == null)
        {
            return NotFound();
        }

        _context.CookbookEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}