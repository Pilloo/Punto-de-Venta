using System.ComponentModel.DataAnnotations;

namespace DTOs.Inventory.Colour;

public record ModifyColourRequest
{
    [Required] public string Name { get; init; } = string.Empty;
}