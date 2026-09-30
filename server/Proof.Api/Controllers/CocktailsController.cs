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

public class CocktailsController : ControllerBase
{
    private readonly ProofDbContext _context;
    private readonly WhatCanIMakeService _whatCanIMakeService;

    public CocktailsController(ProofDbContext context, WhatCanIMakeService whatCanIMakeService)
    {
        _context = context;
        _whatCanIMakeService = whatCanIMakeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCocktails(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] Season? season)

    {
        var query = _context.Cocktails.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(c => c.Category.ToLower() == category.ToLower());
        }

        if (season.HasValue)
        {
            query = query.Where(c => c.CocktailSeasons.Any(cs => cs.Season == season.Value));
        }

        var cocktails = await query
            .Select(c => new CocktailSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                Category = c.Category,
                Glass = c.Glass,
                ImageUrl = c.ImageUrl
            })
            .ToListAsync();

        return Ok(cocktails);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCocktailById(Guid id)
    {
        var cocktail = await _context.Cocktails
            .Include(c => c.CocktailIngredients)
            .ThenInclude(ci => ci.Ingredient)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cocktail == null)
        {
            return NotFound();
        }

        var cocktailDetailDto = new CocktailDetailDto
        {
            Id = cocktail.Id,
            Name = cocktail.Name,
            Category = cocktail.Category,
            Glass = cocktail.Glass,
            ImageUrl = cocktail.ImageUrl,
            Instructions = cocktail.Instructions,
            Ingredients = cocktail.CocktailIngredients.Select(ci => new CocktailIngredientDto
            {
                IngredientId = ci.IngredientId,
                IngredientName = ci.Ingredient.Name,
                Measure = ci.Measure
            }).ToList()
        };

        return Ok(cocktailDetailDto);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Cocktails
            .Select(c => c.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("browse")]
    public async Task<IActionResult> Browse([FromQuery] int perCategory = 4)
    {
        var categories = await _context.Cocktails
            .Select(c => c.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var previews = new List<CategoryPreviewDto>();

        foreach (var category in categories)
        {
            var cocktails = await _context.Cocktails
                .Where(c => c.Category == category)
                .OrderBy(c => c.Name)
                .Take(perCategory)
                .Select(c => new CocktailSummaryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Category = c.Category,
                    Glass = c.Glass,
                    ImageUrl = c.ImageUrl
                })
                .ToListAsync();

            previews.Add(new CategoryPreviewDto { Category = category, Cocktails = cocktails });
        }

        return Ok(previews);
    }

    [HttpGet("what-can-i-make")]
    public async Task<IActionResult> WhatCanIMake([FromQuery] string? ingredients)
    {
        var ingredientNames = (ingredients ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
        var results = await _whatCanIMakeService.FindMakeableCocktailsAsync(ingredientNames);
        return Ok(results);
    }
}