using System.ComponentModel.DataAnnotations;

namespace Models.Inventory
{
    public class Category
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        public string Name { get; set; } = string.Empty;

        public ICollection<Product> Products = new List<Product>();

        [Required]
        public bool IsActive { get; set; } = true;
    }
}
