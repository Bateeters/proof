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
    private const string RefreshCookieName = "refreshToken";

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
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newAccount = new Account
        {
            Email = request.Email,
            PasswordHash = hashedPassword
        };

        _context.Accounts.Add(newAccount);
        await _context.SaveChangesAsync();

        return Ok(await IssueSessionAsync(newAccount));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == request.Email);

        if (account == null || !BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
            return Unauthorized();

        return Ok(await IssueSessionAsync(account));
    }

    // Silently exchanges the httpOnly refresh cookie for a fresh access
    // token — called once when the SPA loads, so a page refresh restores
    // the session instead of logging the user out, without ever putting
    // a long-lived token somewhere JavaScript (and therefore an XSS
    // attack) could read it.
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var rawToken) || string.IsNullOrEmpty(rawToken))
        {
            return Unauthorized();
        }

        var tokenHash = TokenService.HashRefreshToken(rawToken);
        var storedToken = await _context.RefreshTokens
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized();
        }

        if (storedToken.RevokedAt != null)
        {
            // This exact refresh token was already used once (rotation
            // below revokes it) or was logged out — seeing it again means
            // it likely leaked. Defensive response: kill every active
            // refresh token on the account so a real login is required
            // everywhere, not just here.
            var activeTokens = await _context.RefreshTokens
                .Where(t => t.AccountId == storedToken.AccountId && t.RevokedAt == null)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Unauthorized();
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(await IssueSessionAsync(storedToken.Account));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue(RefreshCookieName, out var rawToken) && !string.IsNullOrEmpty(rawToken))
        {
            var tokenHash = TokenService.HashRefreshToken(rawToken);
            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (storedToken != null)
            {
                storedToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/api/auth" });
        return NoContent();
    }

    // Shared by register/login/refresh: issues a fresh access token (JSON
    // body) + a fresh refresh token (httpOnly cookie, rotated on every use).
    private async Task<AuthResponseDto> IssueSessionAsync(Account account)
    {
        var rawRefreshToken = _tokenService.GenerateRefreshTokenValue();

        _context.RefreshTokens.Add(new RefreshToken
        {
            AccountId = account.Id,
            TokenHash = TokenService.HashRefreshToken(rawRefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_tokenService.RefreshTokenExpiryDays)
        });

        await _context.SaveChangesAsync();

        Response.Cookies.Append(RefreshCookieName, rawRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(_tokenService.RefreshTokenExpiryDays)
        });

        return new AuthResponseDto
        {
            Token = _tokenService.GenerateAccessToken(account),
            Account = new AccountDto
            {
                Id = account.Id,
                Email = account.Email,
                CreatedAt = account.CreatedAt
            }
        };
    }
}
