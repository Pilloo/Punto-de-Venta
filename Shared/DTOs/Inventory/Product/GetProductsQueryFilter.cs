namespace DTOs.Inventory.Product;

/// <summary>
/// Represents the filtering criteria for querying products.
/// </summary>
/// <remarks>
/// This class is designed to encapsulate various filtering options that can be applied when querying
/// a collection of products. It supports filtering by textual search, associated brand, category, colour,
/// price range, stock count range, and active state. Additionally, it includes pagination properties.
/// </remarks>
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