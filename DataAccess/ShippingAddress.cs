using System;
using System.Collections.Generic;

namespace HTTL_May_Xay_Dung.DataAccess;

public partial class ShippingAddress
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int UserId { get; set; }

    public int CityId { get; set; }

    public string SpecificAddress { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public virtual City City { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
