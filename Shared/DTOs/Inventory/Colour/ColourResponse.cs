namespace DTOs.Inventory.Colour;

public class ColourResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }

    public static ColourResponse FromEntity(Models.Inventory.Colour colour) => new()
    {
        Id = colour.Id,
        Name = colour.Name,
        Active = colour.IsActive
    };
}