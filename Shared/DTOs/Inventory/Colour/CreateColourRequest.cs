using System.ComponentModel.DataAnnotations;

namespace DTOs.Inventory.Colour;

public class CreateColourRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Name { get; set; } = string.Empty;
}