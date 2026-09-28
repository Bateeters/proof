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
public class SubstitutionsController : ControllerBase
{
    private readonly ProofDbContext _context;

    public SubstitutionsController(ProofDbContext context)
    {
        _context = context;
    }

    [HttpPost("suggest")]
    public async Task<IActionResult> Suggest(SubstitutionRequestDto request)
    {
        // Two-question UX flow ("why swap it — Taste, or Availability? if
        // Availability, is it Cost or Supply?") mapped onto the three-value
        // SubstitutionReason actually stored on the rule table.
        SubstitutionReason reason;
        if (request.Reason == "Taste")
        {
            reason = SubstitutionReason.Taste;
        }
        else if (request.Reason == "Availability" && request.SubReason == "Cost")
        {
            reason = SubstitutionReason.Cost;
        }
        else if (request.Reason == "Availability" && request.SubReason == "Supply")
        {
            reason = SubstitutionReason.Supply;
        }
        else
        {
            return BadRequest(new { message = "reason must be \"Taste\", or \"Availability\" with subReason \"Cost\" or \"Supply\"." });
        }

        var ingredientIsInCocktail = await _context.CocktailIngredients
            .AnyAsync(ci => ci.CocktailId == request.CocktailId && ci.IngredientId == request.IngredientId);

        if (!ingredientIsInCocktail)
        {
            return NotFound();
        }

        var suggestion = await _context.IngredientSubstitutions
            .Where(s => s.SourceIngredientId == request.IngredientId && s.Reason == reason)
            .Select(s => new SubstitutionSuggestionDto
            {
                ReplacementIngredientId = s.ReplacementIngredientId,
                ReplacementIngredientName = s.ReplacementIngredient.Name,
                Notes = s.Notes
            })
            .FirstOrDefaultAsync();

        if (suggestion == null)
        {
            return NotFound();
        }

        return Ok(suggestion);
    }
}
