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
    public class CityController : ControllerBase
    {
        private readonly BfhahziulzpihzqwnwfhContext _context;
        private readonly PaginationSettings _paginationSettings;

        public CityController(IConfiguration configuration, BfhahziulzpihzqwnwfhContext context, IOptions<PaginationSettings> paginationSettings, IWebHostEnvironment environment)
        {
            _context = context;
            _paginationSettings = paginationSettings.Value;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllCity()
        {
            var ct = _context.Cities.ToList();
            if (ct == null)
            {
                return NotFound(new { message = "Không tìm thấy thành phố nào" });
            }
            return Ok(ct);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddCity([FromBody] CityDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var city = await _context.Cities
                .FirstOrDefaultAsync(c => c.Name == dto.Name);

            if (city != null)
            {
                return BadRequest(new { message = "Thành phố này đã tồn tại." });
            }

            var newCity = new City
            {
                Name = dto.Name,
            };

            _context.Cities.Add(newCity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCityById), new { id = newCity.Id }, newCity);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetCityById(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound(new { message = "Thành phố không tồn tại." });
            }
            return Ok(city);
        }

        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> EditCity(int id, [FromBody] CityDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCity = await _context.Cities.FindAsync(id);
            if (existingCity == null)
            {
                return NotFound(new { message = "Thành phố không tồn tại." });
            }

            var duplicateCity = await _context.Cities
                .FirstOrDefaultAsync(c => c.Name == dto.Name && c.Id != id);
            if (duplicateCity != null)
            {
                return BadRequest(new { message = "Tên thành phố đã tồn tại." });
            }

            existingCity.Name = dto.Name;

            _context.Cities.Update(existingCity);
            await _context.SaveChangesAsync();

            return Ok(existingCity);
        }

    }
}
