using System.ComponentModel.DataAnnotations;

namespace Models.Inventory
{
    public class Brand
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public ICollection<Product> Products = new List<Product>();

        public bool IsActive { get; set; } = true;
    }
}
