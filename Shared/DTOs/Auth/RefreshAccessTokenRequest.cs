using System.ComponentModel.DataAnnotations;

namespace DTOs.Auth;

public class RefreshAccessTokenRequest
{
    [Required] public Guid UserId { get; set; } = Guid.Empty;
    [Required] public string RefreshToken { get; set; } = string.Empty;
}