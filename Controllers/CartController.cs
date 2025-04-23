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
    public class CartController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly BfhahziulzpihzqwnwfhContext _context;
        private readonly PaginationSettings _paginationSettings;

        public CartController(IConfiguration configuration, BfhahziulzpihzqwnwfhContext context, IOptions<PaginationSettings> paginationSettings)
        {
            _configuration = configuration;
            _context = context;
            _paginationSettings = paginationSettings.Value;
        }

        [HttpGet("GetCartByUserId/{userId}")]
        public async Task<IActionResult> GetCartByUserId(int userId)
        {
            var customer = await _context.Users
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng." });
            }

            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product) // Join với bảng Product để lấy thông tin sản phẩm
                .Select(c => new CartDTO
                {
                    UserId = userId,
                    Username = c.User.Username,
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    Image = c.Product.Image,
                    Quantity = c.Quantity
                })
                .ToListAsync();

            if (!cartItems.Any())
            {
                return Ok(new { message = "Giỏ hàng đang trống.", cartItems = new List<object>() });
            }


            return Ok(new
            {
                message = "Lấy danh sách sản phẩm trong giỏ hàng thành công.",
                cartItems = cartItems,
            });
        }

        [HttpGet("CountUniqueProducts/{userId}")]
        public async Task<IActionResult> CountUniqueProducts(int userId)
        {
            var customer = await _context.Users
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng." });
            }

            int uniqueProductCount = await _context.Carts
                .Where(c => c.UserId == userId)
                .Select(c => c.ProductId)
                .Distinct()
                .CountAsync();

            return Ok(new
            {
                message = "Lấy tổng số sản phẩm khác nhau trong giỏ hàng thành công.",
                uniqueProductCount = uniqueProductCount
            });
        }



        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart(int userId, int productId, int quantity)
        {
            var customer = await _context.Users
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng." });
            }

            var cartItemsCount = await _context.Carts
                .CountAsync(x => x.UserId == customer.Id);

            if (cartItemsCount >= 20)
            {
                return BadRequest(new { message = "Giỏ hàng đã đạt tối đa 20 sản phẩm." });
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == productId);

            if (product == null)
            {
                return BadRequest(new { message = "Sản phẩm không tồn tại." });
            }

            if (quantity <= 0)
            {
                return BadRequest(new { message = "Số lượng sản phẩm phải lớn hơn 0." });
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(x => x.UserId == customer.Id && x.ProductId == productId);

            if (cartItem != null) // Nếu sản phẩm đã tồn tại trong giỏ hàng
            {
                int newQuantity = cartItem.Quantity + quantity;

                cartItem.Quantity = newQuantity;
            }
            else // Nếu sản phẩm chưa tồn tại trong giỏ hàng
            {

                var newCartItem = new Cart
                {
                    UserId = customer.Id,
                    ProductId = productId,
                    Quantity = quantity
                };

                await _context.Carts.AddAsync(newCartItem);
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm sản phẩm vào giỏ hàng thành công." });
        }


        [HttpPost("UpdateQuantityFromCart")]
        public async Task<IActionResult> UpdateQuantityFromCart(int userId, int productId, int quantity)
        {
            var customer = await _context.Users
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng." });
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(x => x.UserId == customer.Id && x.ProductId == productId);

            if (cartItem == null)
            {
                return BadRequest(new { message = "Sản phẩm không tồn tại trong giỏ hàng." });
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == productId);

            if (product == null)
            {
                return BadRequest(new { message = "Sản phẩm không tồn tại." });
            }

            if (quantity < 0)
            {
                return BadRequest(new { message = "Số lượng không được là số âm." });
            }

            if (quantity <= 0)
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã xóa sản phẩm khỏi giỏ hàng." });
            }

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();

            // Lấy danh sách sản phẩm trong giỏ hàng sau khi cập nhật
            var cartItems = await _context.Carts
                .Where(x => x.UserId == customer.Id)
                .Select(x => new
                {
                    x.ProductId,
                    x.Quantity
                })
                .ToListAsync();


            return Ok(new { message = "Cập nhật số lượng sản phẩm thành công.", cartItems = cartItems });
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(int userId, int productId)
        {
            var customer = await _context.Users
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng." });
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(x => x.UserId == customer.Id && x.ProductId == productId);

            if (cartItem == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm trong giỏ hàng." });
            }

            _context.Carts.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sản phẩm đã được xóa khỏi giỏ hàng." });
        }

        [HttpDelete("DeleteAll")]
        public async Task<IActionResult> DeleteAll(int userId)
        {
            var customer = await _context.Users
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng." });
            }

            var cartItems = await _context.Carts
                .Where(x => x.UserId == customer.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return NotFound(new { message = "Giỏ hàng của khách hàng đang trống." });
            }

            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tất cả sản phẩm đã được xóa khỏi giỏ hàng." });
        }


    }
}
