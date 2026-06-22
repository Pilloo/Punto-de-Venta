using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models
{
    [Index(nameof(BarcodeContent))]
    public class Product
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid BrandId { get; set; }
        public Brand Brand { get; set; } = null!;
        
        public Guid ColourId { get; set; }
        public Colour Colour { get; set; } = null!;

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        [Required(ErrorMessage = "La descripción del producto es obligatoria.")]
        [MaxLength(255, ErrorMessage = "La descripción del producto no puede exceder los 255 carácteres.")]
        public string ItemSummary { get; set; } = string.Empty;

        public string BarcodeContent {  get; set; } = string.Empty;

        public int StockCount { get; set; } = 0;

        public int MinimumStockLevel { get; set; } = 0;

        [Precision(19, 4)]
        public decimal Price = 0;

        public bool IsActive = true;
    }
}
