using Models;

namespace DTOs.Inventory.Brand;

public record BrandResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }

    public static BrandResponse FromEntity(Models.Inventory.Brand brand) => new()
    {
        Id = brand.Id,
        Name = brand.Name,
        Active = brand.IsActive
    };
}