namespace Proof.Api.DTOs;

public class AuthResponseDto
{
    public required string Token { get; set; }
    public required AccountDto Account { get; set; }
}