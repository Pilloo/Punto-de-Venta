using System.ComponentModel.DataAnnotations;

namespace DTOs.Inventory.Product;

public class SetProductStatusRequest
{
    [Required] public bool Status { get; set; } = false;
}