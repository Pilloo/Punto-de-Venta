using DTOs;
using ErrorHandling;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Core.UseCases.Commands
{
    public class RefreshAccessTokenCommand : IRequest<Result<LoginResponse>>
    {
        [Required] public Guid UserId { get; set; } = Guid.Empty;
        [Required] public string RefreshToken { get; set; } = string.Empty;
    }
}
