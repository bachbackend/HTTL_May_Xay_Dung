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


        [HttpGet("GetAllOrderSortByDate")]
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
                .Include(p => p.ShippingAddress)
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
                    PhoneNumber = p.ShippingAddress.PhoneNumber,
                    StatusId = p.OrderStatusId,
                    StatusName = p.OrderStatus.Name,
                    OrderDate = p.OrderDate,
                    //TotalPrice = p.TotalPrice,
                    CityId = p.ShippingAddress.CityId,
                    CityName = p.ShippingAddress.City.Name,
                    SpecificAddress = p.ShippingAddress.SpecificAddress

                })
                    .ToListAsync();

            var result = new
            {
                Orders = orderWithPaging,
                Paging = pagingResult
            };

            return Ok(result);
        }

        [HttpGet("GetOrderByOrderId/{orderId}")]
        public async Task<IActionResult> GetOrderByOrderId(int orderId)
        {
            var order = await _context.Orders
                .Include(p => p.OrderStatus)
                .Include(p => p.User)
                .Include(p => p.ShippingAddress)
                    .ThenInclude(p => p.City)
                .Where(p => p.Id == orderId)
                .Select(p => new OrderDTO
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    CityId = p.ShippingAddress.CityId,
                    CityName = p.ShippingAddress.City.Name,
                    SpecificAddress = p.ShippingAddress.SpecificAddress,
                    PhoneNumber = p.ShippingAddress.PhoneNumber,
                    StatusId = p.OrderStatus.Id,
                    StatusName = p.OrderStatus.Name,
                    OrderDate = p.OrderDate,
                    //TotalPrice = p.TotalPrice

                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound(new { Message = "Không tìm thấy đơn hàng." });
            }

            return Ok(order);
        }

        [HttpGet("GetOrderDetailsByOrderId/{orderId}")]
        public async Task<IActionResult> GetOrderDetailsByOrderId(int orderId)
        {
            var orders = await _context.Orders
                .Where(o => o.Id == orderId)
                .Include(p => p.OrderStatus)
                .Include(p => p.User)
                .Include(p => p.ShippingAddress)
                    .ThenInclude(p => p.City)
                .Include(p => p.OrderDetails)
                    .ThenInclude(p => p.Product)
                        .ThenInclude(p => p.Category)
                .Select(o => new OrderDTO
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    Username = o.User.Username,
                    SpecificAddress = o.ShippingAddress.SpecificAddress,
                    PhoneNumber = o.ShippingAddress.PhoneNumber,
                    CityId = o.ShippingAddress.CityId,
                    CityName = o.ShippingAddress.City.Name,
                    StatusId = o.OrderStatus.Id,
                    StatusName = o.OrderStatus.Name,
                    OrderDate = o.OrderDate,
                    //TotalPrice = o.TotalPrice,

                    // Thông tin chi tiết của OrderDetail
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailDTO
                    {
                        Id = od.Id,
                        OrderId = od.OrderId,
                        ProductId = od.ProductId,
                        ProductName = od.Product.Name,
                        ProductImage = od.Product.Image,
                        Quantity = od.Quantity,
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (orders == null)
            {
                return NotFound(new { Message = "Không tìm thấy chi tiết của đơn hàng." });
            }

            return Ok(orders);
        }

        [HttpGet("GetOrderByUserId/{userId}")]
        public async Task<IActionResult> GetOrderByUserId(
            int userId,
            int pageNumber = 1,
            int? pageSize = null
            )
        {
            int actualPageSize = pageSize ?? _paginationSettings.DefaultPageSize;
            var orders = _context.Orders
                .Include(p => p.OrderStatus)
                .Include(p => p.User)
                .Include(p => p.ShippingAddress)
                    .ThenInclude(p => p.City)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.OrderDate)
                .AsQueryable();

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
                    SpecificAddress = p.ShippingAddress.SpecificAddress,
                    PhoneNumber = p.ShippingAddress.PhoneNumber,
                    CityId = p.ShippingAddress.CityId,
                    CityName = p.ShippingAddress.City.Name,
                    StatusId = p.OrderStatusId,
                    StatusName = p.OrderStatus.Name,
                    OrderDate = p.OrderDate,
                    //TotalPrice = p.TotalPrice,
                })
                .ToListAsync();

            var result = new
            {
                Orders = orderWithPaging,
                Paging = pagingResult
            };

            return Ok(result);
        }


        [HttpPut("UpdateOrderStatus/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromForm] int statusId)
        {
            // Kiểm tra nếu trạng thái là 6 (Hủy đơn hàng), không cho phép cập nhật
            if (statusId == 7)
            {
                return BadRequest(new { message = "Không được thực hiện hủy đơn hàng tại đây." });
            }

            // Kiểm tra sự hợp lệ của trạng thái
            var statusExists = await _context.OrderStatuses.AnyAsync(s => s.Id == statusId);
            if (!statusExists)
            {
                return BadRequest(new { message = "Trạng thái không hợp lệ." });
            }

            // Tìm đơn hàng theo orderId
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            // Cập nhật trạng thái đơn hàng
            order.OrderStatusId = statusId;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message = "Cập nhật trạng thái đơn hàng thành công!",
                    orderId = order.Id,
                    statusId = order.OrderStatusId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi khi cập nhật: {ex.Message}" });
            }
        }

        [HttpPut("CancelOrder/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            // Tìm đơn hàng theo ID
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return Ok(new { message = "Không tìm thấy đơn hàng." });
            }

            // Kiểm tra trạng thái đơn hàng
            if (order.OrderStatusId == 7)
            {
                return Ok(new { message = "Đơn hàng này đã bị hủy trước đó." });
            }

            if (order.OrderStatusId == 4 || order.OrderStatusId == 5 || order.OrderStatusId == 6)
            {
                return Ok(new { message = "Không thể hủy đơn hàng đang giao hoặc đã hoàn thành." });
            }

            var orderDetails = await _context.OrderDetails
                .Where(od => od.OrderId == orderId)
                .ToListAsync();

            foreach (var detail in orderDetails)
            {
                var productItem = await _context.Products
                    .FirstOrDefaultAsync(pi => pi.Id == detail.ProductId);

                if (productItem != null)
                {
                    productItem.SaleQuantity -= detail.Quantity;
                }
            }

            // Cập nhật trạng thái
            order.OrderStatusId = 7;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message = "Đơn hàng đã được hủy thành công!",
                    orderId = order.Id,
                    statusId = order.OrderStatusId,
                });
            }
            catch (Exception ex)
            {
                return Ok(new { message = $"Đã xảy ra lỗi khi hủy đơn hàng: {ex.Message}" });
            }
        }

        [HttpGet("CountOrders")]
        public async Task<IActionResult> CountOrders()
        {
            var quantity = await _context.Orders.CountAsync();

            return Ok(quantity);
        }

        [HttpPut("UpdateTotalPrice/{orderId}")]
        public async Task<IActionResult> UpdateTotalPrice(int orderId, [FromForm] decimal totalPrice)
        {
            // Kiểm tra giá trị totalPrice hợp lệ (không âm)
            if (totalPrice < 0)
            {
                return BadRequest("Tổng giá trị đơn hàng không thể là số âm.");
            }

            // Tìm đơn hàng theo orderId
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            // Cập nhật tổng giá trị đơn hàng
            order.TotalPrice = totalPrice;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message = "Cập nhật tổng giá trị đơn hàng thành công!",
                    orderId = order.Id,
                    totalPrice = order.TotalPrice
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi khi cập nhật tổng giá trị đơn hàng: {ex.Message}");
            }
        }


    }
}
