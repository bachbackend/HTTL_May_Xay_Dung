namespace HTTL_May_Xay_Dung.DTO
{
    public class ArticleRequestUpdate
    {

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public short Status { get; set; }

        public int ArticleCateId { get; set; }
    }
}
