using System;
using System.Collections.Generic;

namespace HTTL_May_Xay_Dung.DataAccess;

public partial class Contact
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Phonenumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Text { get; set; } = null!;

    public short Status { get; set; }

    public DateTime ContactDate { get; set; }
}
