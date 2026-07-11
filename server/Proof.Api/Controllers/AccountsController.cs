using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.DTOs;

namespace Proof.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly ProofDbContext _context;

    public AccountsController(ProofDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        var accounts = await _context.Accounts.ToListAsync();

        var accountDtos = accounts.Select(account => new AccountDto
        {
            Id = account.Id,
            Email = account.Email,
            CreatedAt = account.CreatedAt
        });

        return Ok(accountDtos);
    }
}