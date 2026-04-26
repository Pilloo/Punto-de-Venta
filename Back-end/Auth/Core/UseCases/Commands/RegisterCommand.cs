using ErrorHandling;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.UseCases.Commands
{
    public class RegisterCommand : IRequest<Result<Unit>>
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required] 
        public string GivenName { get; set; } = string.Empty;
        
        [Required] 
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [DataType(DataType.Password)] 
        public string Password { get; set; } = string.Empty;
    }
}
