using System;
using System.Collections.Generic;

namespace HTTL_May_Xay_Dung.DataAccess;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public short Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastLogin { get; set; }

    public short Status { get; set; }

    public string Phonenumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ShippingAddress> ShippingAddresses { get; set; } = new List<ShippingAddress>();
}
