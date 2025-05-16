using HTTL_May_Xay_Dung.DataAccess;

namespace HTTL_May_Xay_Dung.DTO
{
    public class OrderDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string SpecificAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public int StatusId { get; set; }
        public string StatusName { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        //public decimal? TotalPrice { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; } = null!;
        public List<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();
    }

    public class OrderDetailDTO
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductImage { get; set; } = null!;

        public int Quantity { get; set; }
    }
}
