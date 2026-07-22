using System.ComponentModel.DataAnnotations;

namespace DTOs.Users;

public class ModifyUserRequest
{
    [Required] public Guid UserId { get; set; } = Guid.Empty;

    public string GivenName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    [DataType(DataType.Password)] public string OldPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Contraseña nueva")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "La {0} debe tener entre {2} y {1} caracteres.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
        ErrorMessage =
            "La contraseña debe contener al menos una letra mayúscula, una minúscula, un número y un carácter especial.")]
    public string NewPassword { get; set; } = string.Empty;
}