using HTTL_May_Xay_Dung.DataAccess;
using HTTL_May_Xay_Dung.DTO;
using HTTL_May_Xay_Dung.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HTTL_May_Xay_Dung.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly BfhahziulzpihzqwnwfhContext _context;
        private readonly PaginationSettings _paginationSettings;
        private readonly IWebHostEnvironment _environment;

        public ArticleController(IConfiguration configuration, BfhahziulzpihzqwnwfhContext context, IOptions<PaginationSettings> paginationSettings, IWebHostEnvironment environment)
        {
            _context = context;
            _paginationSettings = paginationSettings.Value;
            _environment = environment;
        }

        [HttpGet("GetAllArticle")]
        public async Task<IActionResult> GetAllArticle(
            int pageNumber = 1,
            int? pageSize = null,
            int? status = null,
            string? title = null,
            int? categoryId = null
            )
        {
            int actualPageSize = pageSize ?? _paginationSettings.DefaultPageSize;
            var articles = _context.Articles
                .Include(p => p.ArticleCate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                articles = articles.Where(p => p.Title.Contains(title));
            }

            if (categoryId.HasValue)
            {
                articles = articles.Where(p => p.ArticleCateId == categoryId.Value);
            }

            if (status.HasValue)
            {
                articles = articles.Where(p => p.Status == status.Value);
            }

            int totalArticleCount = await articles.CountAsync();


            int totalPageCount = (int)Math.Ceiling(totalArticleCount / (double)actualPageSize);
            int nextPage = pageNumber + 1 > totalPageCount ? pageNumber : pageNumber + 1;
            int previousPage = pageNumber - 1 < 1 ? pageNumber : pageNumber - 1;

            var pagingResult = new PagingReturn
            {
                TotalPageCount = totalPageCount,
                CurrentPage = pageNumber,
                NextPage = nextPage,
                PreviousPage = previousPage
            };

            List<ArticleReturnDTO> articletWithPaging = await articles
                .Skip((pageNumber - 1) * actualPageSize)
                .Take(actualPageSize)
                .Select(p => new ArticleReturnDTO
                {
                    Id = p.Id,
                    Thumbnail = p.Thumbnail,
                    Title = p.Title,
                    Content = p.Content,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    ArticleCateId = p.ArticleCateId,
                    ArticleCateName = p.ArticleCate.Name,
                })
            .ToListAsync();

            var result = new
            {
                Articles = articletWithPaging,
                Paging = pagingResult
            };

            return Ok(result);
        }
    }
}
