using HTTL_May_Xay_Dung.DataAccess;
using HTTL_May_Xay_Dung.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HTTL_May_Xay_Dung.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShippingAddressController : ControllerBase
    {
        private readonly BfhahziulzpihzqwnwfhContext _context;
        private readonly PaginationSettings _paginationSettings;

        public ShippingAddressController(IConfiguration configuration, BfhahziulzpihzqwnwfhContext context, IOptions<PaginationSettings> paginationSettings)
        {
            _context = context;
            _paginationSettings = paginationSettings.Value;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllShippingAddress()
        {
            var sa = _context.ShippingAddresses.ToList();
            if (sa == null)
            {
                return NotFound(new { message = "Không tìm thấy địa chỉ giao hàng nào." });
            }
            return Ok(sa);
        }

        [HttpGet("GetByUser/{userId}")]
        public async Task<IActionResult> GetShippingAddressByUser(int userId)
        {
            var shippingAddresses = _context.ShippingAddresses
                .Include(sa => sa.City)
                .Where(sa => sa.UserId == userId)
                .Select(sa => new
                {
                    sa.Id,
                    sa.UserId,
                    sa.CityId,
                    sa.City.Name,
                    sa.SpecificAddress,
                    sa.PhoneNumber,
                })
                .ToList();

            if (!shippingAddresses.Any())
            {
                return NotFound(new { message = "Không tìm thấy địa chỉ giao hàng nào cho người dùng này." });
            }

            return Ok(new
            {
                message = "Lấy danh sách địa chỉ giao hàng thành công.",
                shippingAddresses = shippingAddresses
            });
        }

    }
}
