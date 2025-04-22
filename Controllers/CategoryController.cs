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
    public class CategoryController : ControllerBase
    {
        private readonly BfhahziulzpihzqwnwfhContext _context;
        private readonly PaginationSettings _paginationSettings;

        public CategoryController(IConfiguration configuration, BfhahziulzpihzqwnwfhContext context, IOptions<PaginationSettings> paginationSettings, IWebHostEnvironment environment)
        {
            _context = context;
            _paginationSettings = paginationSettings.Value;
        }

        [HttpGet("GetAll")]
        public IActionResult GetAllCategory()
        {
            return Ok(_context.Categories.AsQueryable());

        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddBlogCategory([FromBody] CategoryDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == dto.Name);

            if (existingCategory != null)
            {
                return BadRequest(new { message = "Category này đã tồn tại." });
            }

            var newCategory = new Category
            {
                Name = dto.Name,
                ParentId = dto.ParentId,
            };

            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.Id }, newCategory);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> EditCategory(int id, [FromBody] CategoryDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound(new { message = "Category không tồn tại." });
            }

            var duplicateCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == dto.Name && c.Id != id);
            if (duplicateCategory != null)
            {
                return BadRequest(new { message = "Tên Category đã tồn tại." });
            }

            existingCategory.Name = dto.Name;
            existingCategory.ParentId = dto.ParentId;

            _context.Categories.Update(existingCategory);
            await _context.SaveChangesAsync();

            return Ok(existingCategory);
        }

    }
}
