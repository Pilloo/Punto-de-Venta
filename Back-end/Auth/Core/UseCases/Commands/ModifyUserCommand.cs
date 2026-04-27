using DTOs;
using ErrorHandling;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.UseCases.Commands
{
    public class ModifyUserCommand : IRequest<Result<LoginResponse>>
    {
        [Required] public Guid UserId { get; set; } = Guid.Empty;

        public string GivenName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
