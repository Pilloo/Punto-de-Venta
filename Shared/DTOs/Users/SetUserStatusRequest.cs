using System.ComponentModel.DataAnnotations;

namespace DTOs.Users;

public class SetUserStatusRequest
{
    [Required] public Guid UserId { get; set; } = Guid.Empty;
    [Required] public bool Status { get; set; }
}