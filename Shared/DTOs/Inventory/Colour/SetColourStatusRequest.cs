using System.ComponentModel.DataAnnotations;

namespace DTOs.Inventory.Colour;

public class SetColourStatusRequest
{
    [Required] public bool Status { get; set; } = false;
}