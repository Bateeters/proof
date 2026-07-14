using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proof.Api.Data;
using Proof.Api.DTOs;
using Proof.Api.Models;
using Proof.Api.Services;

namespace Proof.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ProofDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(ProofDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
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

        // return the new account as an AuthResponseDto, wrapped in Ok(...)
        return Ok(new AuthResponseDto
        {
            Token = _tokenService.GenerateToken(newAccount),
            Account = new AccountDto   
            {
                Id = newAccount.Id,
                Email = newAccount.Email,
                CreatedAt = newAccount.CreatedAt
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == request.Email);
        
        if (account == null || !BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
            return Unauthorized();

        var token = _tokenService.GenerateToken(account);
        return Ok(new AuthResponseDto
        {
            Token = token,
            Account = new AccountDto
            {
                Id = account.Id,
                Email = account.Email,
                CreatedAt = account.CreatedAt
            }
        });
    }
}