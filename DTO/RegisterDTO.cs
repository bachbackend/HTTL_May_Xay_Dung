namespace HTTL_May_Xay_Dung.DTO
{
    public class RegisterDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public short Role {  get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? LastLogin { get; set;}
        public short status { get; set; }
    }
}
