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
}