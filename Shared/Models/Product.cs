using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Models
{

    [Index(nameof(BarcodeContent), IsUnique = true)]
    public class Product
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? BrandId { get; set; }
        public Brand? Brand { get; set; } = null!;

        public Guid? ColourId { get; set; }
        public Colour? Colour { get; set; } = null!;

        public Guid? CategoryId { get; set; }
        public Category? Category { get; set; } = null!;

        public string ItemSummary { get; set; } = string.Empty;

        public string BarcodeContent { get; set; } = string.Empty;

        public int StockCount { get; set; } = 0;

        public int MinimumStockLevel { get; set; } = 0;

        [Precision(19, 4)] public decimal Price { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}