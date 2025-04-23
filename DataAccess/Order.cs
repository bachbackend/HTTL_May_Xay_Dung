using System;
using System.Collections.Generic;

namespace HTTL_May_Xay_Dung.DataAccess;

public partial class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int OrderStatusId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal? TotalPrice { get; set; }

    public int ShippingAddressId { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual OrderStatus OrderStatus { get; set; } = null!;

    public virtual ShippingAddress ShippingAddress { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
