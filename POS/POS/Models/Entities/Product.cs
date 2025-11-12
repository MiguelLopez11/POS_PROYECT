using System.ComponentModel.DataAnnotations;

namespace POS.Models.Entities
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public required decimal Price { get; set; }
        public required float Quantity { get; set; }
        public bool isAvailable { get; set; } = true;
    }
}
