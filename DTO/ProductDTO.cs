namespace HTTL_May_Xay_Dung.DTO
{
    public class ProductDTO
    {
        public class ProductReturnDTO
        {
            public int Id { get; set; }

            public int CategoryId { get; set; }

            public string Name { get; set; } = null!;

            public string Image { get; set; } = null!;
            public int? SaleQuantity { get; set; }
            public string Description { get; set; } = null!;
            public decimal? Price { get; set; }

            public short Status { get; set; }

            public DateTime? CreatedAt { get; set; }

            public string CategoryName { get; set; } = null!;
        }

        public class ProductRequestDTO
        {
            public int CategoryId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public sbyte Status { get; set; }
        }
    }
}
