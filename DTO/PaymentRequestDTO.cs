namespace HTTL_May_Xay_Dung.DTO
{
    public class PaymentRequestDTO
    {
        public int UserId { get; set; }
        public int? ShippingAddressId { get; set; } // Địa chỉ đã có
        public NewShippingAddressDTO? NewShippingAddress { get; set; } // Địa chỉ mới (nếu có)
    }
    public class NewShippingAddressDTO
    {
        public int CityId { get; set; }
        public string SpecificAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
}
