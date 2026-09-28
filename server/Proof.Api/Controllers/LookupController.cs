using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.DTOs;

namespace Proof.Api.Controllers;

// Read-only reference data (Spirit, FlavorTag) that the frontend needs to
// render selection UI (e.g. "which spirits do you like/dislike") — not
// specific to any profile, so it doesn't live under ProfilesController.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookupController : ControllerBase
{
    private readonly ProofDbContext _context;

    public LookupController(ProofDbContext context)
    {
        _context = context;
    }

    [HttpGet("spirits")]
    public async Task<IActionResult> GetSpirits()
    {
        var spirits = await _context.Spirits
            .OrderBy(s => s.Name)
            .Select(s => new LookupItemDto { Id = s.Id, Name = s.Name })
            .ToListAsync();

        return Ok(spirits);
    }

    [HttpGet("flavor-tags")]
    public async Task<IActionResult> GetFlavorTags()
    {
        var flavorTags = await _context.FlavorTags
            .OrderBy(f => f.Name)
            .Select(f => new LookupItemDto { Id = f.Id, Name = f.Name })
            .ToListAsync();

        return Ok(flavorTags);
    }
}
