using Microsoft.AspNetCore.Mvc;
using Proof.Api.Data;
using Proof.Api.DTOs;
using Proof.Api.Models;

namespace Proof.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ProofDbContext _context;

    public AuthController(ProofDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto request)
    {
        // hash request.Password with BCrypt
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // build a new Account from request.Email + the hashed password
        var newAccount = new Account
        {
            Email = request.Email,
            PasswordHash = hashedPassword
        };

        // stage it for saving
        _context.Accounts.Add(newAccount);

        // actually write it to the database
        await _context.SaveChangesAsync();

        // return the new account as an AccountDto, wrapped in Ok(...)
        return Ok(new AccountDto
        {
            Id = newAccount.Id,
            Email = newAccount.Email,
            CreatedAt = newAccount.CreatedAt
        });
    }
}