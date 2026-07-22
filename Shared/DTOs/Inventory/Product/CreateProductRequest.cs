using System.ComponentModel.DataAnnotations;

namespace DTOs.Inventory.Product;

public class CreateProductRequest
{
    [Required(ErrorMessage = "Es necesario asociar una marca al producto.")]
    public Guid? BrandId { get; set; } = Guid.Empty;

    [Required(ErrorMessage = "Es necesario asociar una color al producto.")]
    public Guid? ColourId { get; set; } = Guid.Empty;

    [Required(ErrorMessage = "Es necesario asociar una categoria al producto.")]
    public Guid? CategoryId { get; set; } = Guid.Empty;

    [Required(ErrorMessage = "Es necesario añadir una descripción del producto.")]
    public string ItemSummary { get; set; } = string.Empty;

    [Required(ErrorMessage = "Es necesario añadir o generar un código de barras para el producto.")]
    public string BarcodeContent { get; set; } = string.Empty;

    public int StockCount { get; set; } = 0;

    [Required(ErrorMessage = "Es necesario establecer un límite de inventario permitido.")]
    public int MinimumStockLevel { get; set; } = 0;

    [Required(ErrorMessage = "Es necesario asignar un precio al producto.")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Price { get; set; } = 0;
}