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
    public class ArticleCateController : ControllerBase
    {
        private readonly BfhahziulzpihzqwnwfhContext _context;
        private readonly PaginationSettings _paginationSettings;

        public ArticleCateController(IConfiguration configuration, BfhahziulzpihzqwnwfhContext context, IOptions<PaginationSettings> paginationSettings)
        {
            _context = context;
            _paginationSettings = paginationSettings.Value;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllArticleCategory()
        {
            var ac = _context.ArticleCates.ToList();
            if (ac == null)
            {
                return NotFound(new { message = "Không tìm thấy danh mục nào." });
            }
            return Ok(ac);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddAC([FromBody] AcRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ac = await _context.ArticleCates
                .FirstOrDefaultAsync(c => c.Name == dto.Name);

            if (ac != null)
            {
                return BadRequest(new { message = "Danh mục này đã tồn tại." });
            }

            var newAc = new ArticleCate
            {
                Name = dto.Name,
            };

            _context.ArticleCates.Add(newAc);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetACById), new { id = newAc.Id }, newAc);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetACById(int id)
        {
            var ac = await _context.ArticleCates.FindAsync(id);
            if (ac == null)
            {
                return NotFound(new { message = "Danh mục không tồn tại." });
            }
            return Ok(ac);
        }

        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> EditAC(int id, [FromBody] AcRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingAc = await _context.ArticleCates.FindAsync(id);
            if (existingAc == null)
            {
                return NotFound(new { message = "Danh mục không tồn tại." });
            }

            var duplicateAC = await _context.ArticleCates
                .FirstOrDefaultAsync(c => c.Name == dto.Name && c.Id != id);
            if (duplicateAC != null)
            {
                return BadRequest(new { message = "Tên danh mục đã tồn tại." });
            }

            existingAc.Name = dto.Name;

            _context.ArticleCates.Update(existingAc);
            await _context.SaveChangesAsync();

            return Ok(existingAc);
        }
    }
}
