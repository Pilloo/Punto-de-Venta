using System.ComponentModel.DataAnnotations;

namespace DTOs.Inventory.Brand;

public record ModifyBrandRequest
{
    [Required] public string Name { get; init; } = string.Empty;
}