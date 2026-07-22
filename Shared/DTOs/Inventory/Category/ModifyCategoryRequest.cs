using System.ComponentModel.DataAnnotations;

namespace DTOs.Inventory.Category;

public record ModifyCategoryRequest
{
    [Required] public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}