namespace HTTL_May_Xay_Dung.DTO
{
    public class ContactRequest
    {
        public string Username { get; set; } = null!;

        public string Phonenumber { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Text { get; set; } = null!;

        public short Status { get; set; }

        public DateTime ContactDate { get; set; }
    }
}
