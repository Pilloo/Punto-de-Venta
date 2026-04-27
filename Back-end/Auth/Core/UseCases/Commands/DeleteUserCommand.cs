using ErrorHandling;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Core.UseCases.Commands
{
    public class DeleteUserCommand : IRequest<Result<Unit>>
    {
        [Required] public Guid UserId { get; set; } = Guid.Empty;
    }
}
