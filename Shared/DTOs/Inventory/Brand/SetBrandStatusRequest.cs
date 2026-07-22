using System.ComponentModel.DataAnnotations;

namespace DTOs.Inventory.Brand;

public class SetBrandStatusRequest
{
    [Required] public bool Status { get; set; } = false;
}