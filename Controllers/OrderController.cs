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
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly BfhahziulzpihzqwnwfhContext _context;
        private readonly PaginationSettings _paginationSettings;

        public OrderController(IConfiguration configuration, BfhahziulzpihzqwnwfhContext context, IOptions<PaginationSettings> paginationSettings)
        {
            _configuration = configuration;
            _context = context;
            _paginationSettings = paginationSettings.Value;
        }


        [HttpGet("GetAllOrder")]
        public async Task<IActionResult> GetAllOrder(
            int pageNumber = 1,
            int? pageSize = null,
            int? status = null,
            string? name = null,
            DateTime? startDate = null,   // Lọc theo ngày bắt đầu
            DateTime? endDate = null     // Lọc theo ngày kết thúc
                                         //int? categoryId = null
            )
        {
            int actualPageSize = pageSize ?? _paginationSettings.DefaultPageSize;
            var orders = _context.Orders
                .Include(p => p.OrderStatus)
                .Include(p => p.User)
                .Include(p => p.ShippingAddresses)
                    .ThenInclude(p => p.City)
                .AsQueryable();
            if (!string.IsNullOrEmpty(name))
            {
                orders = orders.Where(p => p.User.Username.Contains(name));
            }


            if (status.HasValue)
            {
                orders = orders.Where(p => p.Id == status.Value);
            }

            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= startDate.Value);
            }

            // Lọc theo ngày kết thúc nếu có giá trị
            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= endDate.Value);
            }

            orders = orders.OrderByDescending(p => p.OrderDate);

            int totalOrderCount = await orders.CountAsync();


            int totalPageCount = (int)Math.Ceiling(totalOrderCount / (double)actualPageSize);
            int nextPage = pageNumber + 1 > totalPageCount ? pageNumber : pageNumber + 1;
            int previousPage = pageNumber - 1 < 1 ? pageNumber : pageNumber - 1;

            var pagingResult = new PagingReturn
            {
                TotalPageCount = totalPageCount,
                CurrentPage = pageNumber,
                NextPage = nextPage,
                PreviousPage = previousPage
            };

            List<OrderDTO> orderWithPaging = await orders
                .Skip((pageNumber - 1) * actualPageSize)
                .Take(actualPageSize)
                .Select(p => new OrderDTO
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    PhoneNumber = p.User.Phonenumber,
                    StatusId = p.OrderStatusId,
                    StatusName = p.OrderStatus.Name,
                    OrderDate = p.OrderDate,
                    TotalPrice = p.TotalPrice,
                    CityId = p.ShippingAddresse

                })
                    .ToListAsync();

            var result = new
            {
                Orders = orderWithPaging,
                Paging = pagingResult
            };

            return Ok(result);
        }
    }
}
