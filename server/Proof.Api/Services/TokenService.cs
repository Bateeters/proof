using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Proof.Api.Models;

namespace Proof.Api.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(Account account)
    {
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]!));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, account.Email),
        };

        // Only added for admins, not as "role": "User" for everyone else — keeps
        // non-admin tokens minimal and makes an admin token easy to spot if it
        // ever needs auditing.
        if (account.IsAdmin)
        {
            claims.Add(new Claim("role", "Admin"));
        }

        var expiryMinutes = double.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"]!);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Deliberately NOT a JWT — a refresh token doesn't need to carry claims,
    // it just needs to be an unguessable value the server can look up and
    // validate against the RefreshTokens table (letting it be revoked, which
    // a self-contained JWT couldn't be without a separate blocklist anyway).
    public string GenerateRefreshTokenValue()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public double RefreshTokenExpiryDays => double.Parse(_configuration["Jwt:RefreshTokenExpiryDays"]!);

    // Only the hash is ever stored (see RefreshToken.TokenHash) -- SHA-256 is
    // appropriate here (unlike BCrypt for passwords) because the raw value
    // is already a long, high-entropy random string, not something a
    // brute-force/dictionary attack could feasibly guess.
    public static string HashRefreshToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
