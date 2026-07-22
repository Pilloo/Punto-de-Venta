namespace DTOs.Inventory.Product;

/// <summary>
/// Represents a set of filters that can be applied when querying a list of products.
/// </summary>
public class GetProductsQueryFilter
{
    public string? SearchTerm { get; set; } = string.Empty;
    
    public Guid? BrandId { get; set; } = Guid.Empty;
    public Guid? CategoryId { get; set; } = Guid.Empty;
    public Guid? ColourId { get; set; } = Guid.Empty;
    
    public RangeFilter<decimal>? PriceRange { get; set; }
    
    public RangeFilter<int>? StockCountRange { get; set; }
    
    public bool? IsActive { get; set; } = true;
    
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}