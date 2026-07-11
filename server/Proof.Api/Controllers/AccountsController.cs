using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;

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
        return Ok(await _context.Accounts.ToListAsync());
    }
}