using Microsoft.AspNetCore.Identity;

namespace HTTL_May_Xay_Dung.DTO
{
    public class RegisterAdminDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phonenumber { get; set; }
    }
}
