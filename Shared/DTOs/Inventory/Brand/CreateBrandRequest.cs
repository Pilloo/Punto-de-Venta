using System.ComponentModel.DataAnnotations;

namespace DTOs.Inventory.Brand;

public class CreateBrandRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Name { get; set; } = string.Empty;
}