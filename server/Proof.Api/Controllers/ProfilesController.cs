using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.DTOs;
using Proof.Api.Models;

namespace Proof.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfilesController : ControllerBase
{
    private readonly ProofDbContext _context;

    public ProfilesController(ProofDbContext context)
    {
        _context = context;
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
}