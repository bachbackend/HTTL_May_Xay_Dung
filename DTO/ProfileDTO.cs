namespace HTTL_May_Xay_Dung.DTO
{
    public class ProfileDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phonenumber { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public short Role { get; set; }
    }
}
