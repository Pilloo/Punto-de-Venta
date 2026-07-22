using System.ComponentModel.DataAnnotations;

namespace DTOs.Inventory.category;

public class SetCategoryStatusRequest
{
    [Required] public bool Status { get; set; } = false;
}