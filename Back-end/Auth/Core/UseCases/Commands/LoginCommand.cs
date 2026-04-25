using MediatR;
using ErrorHandling;
using DTOs;
using System.ComponentModel.DataAnnotations;

namespace Core.UseCases.Commands
{
    public class LoginCommand : IRequest<Result<LoginResponse>>
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
        [Required] public bool RememberMe { get; set; } = false;
    }
}
