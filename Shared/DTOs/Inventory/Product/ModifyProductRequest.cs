using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DTOs.Inventory.Product;

public class ModifyProductRequest
{
    [Required(ErrorMessage = "El id del producto es obligatorio.")]
    public Guid Id { get; set; }
    
    public Guid? BrandId { get; set; }
    public Guid? ColourId { get; set; }
    public Guid? CategoryId { get; set; }
    
    public string? ItemSummary { get; set; }
    
    public string? BarcodeContent { get; set; }
    
    public int? StockCount { get; set; }
    public int? MinimumStockLevel { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal? Price { get; set; }
}