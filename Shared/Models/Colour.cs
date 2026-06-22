using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models
{
    public class Colour
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "El nombre del color es obligatorio.")]
        public string Name { get; set; } = string.Empty;

        public ICollection<Product> Products = new List<Product>();

        [Required]
        public bool IsActive { get; set; } = true;
    }
}
