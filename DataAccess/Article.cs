using System;
using System.Collections.Generic;

namespace HTTL_May_Xay_Dung.DataAccess;

public partial class Article
{
    public int Id { get; set; }

    public string Thumbnail { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public short Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int ArticleCateId { get; set; }

    public virtual ArticleCate ArticleCate { get; set; } = null!;
}
