namespace HTTL_May_Xay_Dung.DTO
{
    public class ArticleRequest
    {
        public string Thumbnail { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public short Status { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int ArticleCateId { get; set; }
    }
}
