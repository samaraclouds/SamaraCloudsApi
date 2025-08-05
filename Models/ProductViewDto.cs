namespace SamaraCloudsApi.Models
{
    public class ProductViewDto
    {
        public int Id { get; set; }
        public string ProductCode { get; set; } = default!;
        public string ProductName { get; set; } = default!;
        public string Barcode { get; set; } = default!;
        public string CategoryName { get; set; } = default!;
        public string UnitName { get; set; } = default!;
        public decimal Price { get; set; }
        public int MinimumStock { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
