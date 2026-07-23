using DTOs.Inventory.Brand;
using DTOs.Inventory.Category;
using DTOs.Inventory.Colour;

namespace DTOs.Inventory.Product;

public class ProductResponse
{
    public Guid Id { get; init; }

    public BrandResponse? Brand { get; init; }
    public ColourResponse? Colour { get; init; }
    public CategoryResponse? Category { get; init; }

    public string ItemSummary { get; init; } = string.Empty;

    public string BarcodeContent { get; init; } = string.Empty;

    public int StockCount { get; init; }

    public decimal Price { get; init; }

    public bool Active { get; init; }

    public static ProductResponse FromEntity(Models.Inventory.Product product) => new()
    {
        Id = product.Id,
        ItemSummary = product.ItemSummary,
        BarcodeContent = product.BarcodeContent,
        StockCount = product.StockCount,
        Price = product.Price,
        Active = product.IsActive,
        Brand = BrandResponse.FromEntity(product.Brand!),
        Colour = ColourResponse.FromEntity(product.Colour!),
        Category = CategoryResponse.FromEntity(product.Category!)
    };
}