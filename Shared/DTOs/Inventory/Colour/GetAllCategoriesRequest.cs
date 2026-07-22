namespace DTOs.Inventory.Colour;

public class GetAllColoursRequest
{
    public bool? Active { get; set; }
    public bool? IncludeInactive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}