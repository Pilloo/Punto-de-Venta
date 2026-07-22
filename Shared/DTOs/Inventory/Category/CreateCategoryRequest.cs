using System.ComponentModel.DataAnnotations;

namespace DTOs.Inventory.Category;

public class CreateCategoryRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Name { get; set; } = string.Empty;
}