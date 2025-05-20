using HTTL_May_Xay_Dung.DataAccess;
using HTTL_May_Xay_Dung.DTO;
using HTTL_May_Xay_Dung.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HTTL_May_Xay_Dung.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderStatusController : ControllerBase
    {
        private readonly BfhahziulzpihzqwnwfhContext _context;
        private readonly PaginationSettings _paginationSettings;

        public OrderStatusController(BfhahziulzpihzqwnwfhContext context, IOptions<PaginationSettings> paginationSettings)
        {
            _context = context;
            _paginationSettings = paginationSettings.Value;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllOrderStatus()
        {
            var os = _context.OrderStatuses.ToList();
            if (os == null)
            {
                return NotFound(new { Message = "Không tìm thấy trạng thái đơn hàng nào." });
            }
            return Ok(os);
        }

        [Authorize(Policy = "AdminAndManagerOnly")]
        [HttpPost("Add")]
        public async Task<IActionResult> AddOrderStatus([FromBody] OrderStatusRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var os = await _context.OrderStatuses
                .FirstOrDefaultAsync(c => c.Name == dto.Name);

            if (os != null)
            {
                return BadRequest(new { Message = "Trạng thái đơn hàng này đã tồn tại." });
            }

            var newOs = new OrderStatus
            {
                Name = dto.Name,
            };

            _context.OrderStatuses.Add(newOs);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOsById), new { id = newOs.Id }, newOs);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetOsById(int id)
        {
            var os = await _context.OrderStatuses.FindAsync(id);
            if (os == null)
            {
                return NotFound(new { Message = "Trạng thái đơn hàng không tồn tại." });
            }
            return Ok(os);
        }

        [Authorize(Policy = "AdminAndManagerOnly")]
        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> EditOs(int id, [FromBody] OrderStatusRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingOs = await _context.OrderStatuses.FindAsync(id);
            if (existingOs == null)
            {
                return NotFound(new { Message = "Trạng thái đơn hàng không tồn tại." });
            }

            var duplicateOs = await _context.OrderStatuses
                .FirstOrDefaultAsync(c => c.Name == dto.Name && c.Id != id);
            if (duplicateOs != null)
            {
                return BadRequest(new { Message = "Trạng thái đơn hàng đã tồn tại." });
            }

            existingOs.Name = dto.Name;

            _context.OrderStatuses.Update(existingOs);
            await _context.SaveChangesAsync();

            return Ok(existingOs);
        }

        [Authorize(Policy = "AdminAndManagerOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOs(int id)
        {
            var existingOs = await _context.OrderStatuses.FindAsync(id);
            if (existingOs == null)
            {
                return NotFound(new { message = "Trạng thái đơn hàng không tồn tại." });
            }

            _context.OrderStatuses.Remove(existingOs);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa trạng thái đơn hàng thành công." });
        }
    }
}
