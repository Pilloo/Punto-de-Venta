namespace DTOs.Inventory.Brand;

public class GetAllBrandsRequest
{
    public bool? Active { get; set; } = false;
    public bool? IncludeInactive { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}