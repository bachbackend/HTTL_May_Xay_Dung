using HTTL_May_Xay_Dung.DataAccess;
using HTTL_May_Xay_Dung.DTO;
using HTTL_May_Xay_Dung.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using static HTTL_May_Xay_Dung.DTO.ProductDTO;

namespace HTTL_May_Xay_Dung.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly BfhahziulzpihzqwnwfhContext _context;
        private readonly PaginationSettings _paginationSettings;
        private readonly IWebHostEnvironment _environment;

        public ProductController(IConfiguration configuration, BfhahziulzpihzqwnwfhContext context, IOptions<PaginationSettings> paginationSettings, IWebHostEnvironment environment)
        {
            _context = context;
            _paginationSettings = paginationSettings.Value;
            _environment = environment;
        }


        [HttpGet("GetAllProduct")]
        public async Task<IActionResult> GetAllProduct(
            int pageNumber = 1,
            int? pageSize = null,
            int? status = null,
            string? name = null,
            int? categoryId = null,
            decimal minPrice = 0,
            decimal maxPrice = 0,
            string? sortBy = "id",
            string? sortOrder = "asc"
            )
        {
            int actualPageSize = pageSize ?? _paginationSettings.DefaultPageSize;
            var products = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                products = products.Where(p => p.Name.Contains(name));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            if (status.HasValue)
            {
                products = products.Where(p => p.Status == status.Value);
            }
            if (minPrice > 0)
            {
                products = products.Where(p => p.ProductPrice >= minPrice);
            }
            if (maxPrice > 0)
            {
                products = products.Where(p => p.ProductPrice <= maxPrice);
            }

            if (sortBy?.ToLower() == "name")
            {
                products = sortOrder.ToLower() == "desc"
                    ? products.OrderByDescending(p => p.Name)
                    : products.OrderBy(p => p.Name);
            }
            else if (sortBy?.ToLower() == "saleQuantity")
            {
                products = sortOrder.ToLower() == "desc"
                    ? products.OrderByDescending(p => p.SaleQuantity)
                    : products.OrderBy(p => p.SaleQuantity);
            }
            else if (sortBy?.ToLower() == "createDate")
            {
                products = sortOrder.ToLower() == "desc"
                    ? products.OrderByDescending(p => p.CreatedAt)
                    : products.OrderBy(p => p.CreatedAt);
            }
            else // Sắp xếp mặc định theo Id
            {
                products = sortOrder.ToLower() == "desc"
                    ? products.OrderByDescending(p => p.Id)
                    : products.OrderBy(p => p.Id);
            }

            int totalProductCount = await products.CountAsync();


            int totalPageCount = (int)Math.Ceiling(totalProductCount / (double)actualPageSize);
            int nextPage = pageNumber + 1 > totalPageCount ? pageNumber : pageNumber + 1;
            int previousPage = pageNumber - 1 < 1 ? pageNumber : pageNumber - 1;

            var pagingResult = new PagingReturn
            {
                TotalPageCount = totalPageCount,
                CurrentPage = pageNumber,
                NextPage = nextPage,
                PreviousPage = previousPage
            };

            List<ProductReturnDTO> productWithPaging = await products
                .Skip((pageNumber - 1) * actualPageSize)
                .Take(actualPageSize)
                .Select(p => new ProductReturnDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    Image = p.Image,
                    SaleQuantity = p.SaleQuantity,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Specifications = p.Specifications,
                    ProductPrice = p.ProductPrice,
                    BasePrice = p.BasePrice,
                })
            .ToListAsync();

            var result = new
            {
                Products = productWithPaging,
                Paging = pagingResult
            };

            return Ok(result);
        }


        [HttpPost("addProduct")]
        public async Task<IActionResult> AddProduct(IFormFile file, [FromForm] ProductRequestDTO model)
        {
            // Kiểm tra nếu ảnh được upload
            if (file == null || file.Length == 0)
            {
                return BadRequest("No image uploaded.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Invalid file type.");
            }

            // Lưu ảnh vào thư mục
            var fileName = Guid.NewGuid() + extension;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/product", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Tạo một sản phẩm mới và lưu vào database
            var product = new Product
            {
                CategoryId = model.CategoryId,
                Name = model.Name,
                Image = fileName,
                Description = model.Description,
                Specifications = model.Specifications,
                Status = model.Status,
                CreatedAt = DateTime.UtcNow,
                ProductPrice = model.ProductPrice,
                BasePrice = model.BasePrice,
            };

            // Lưu sản phẩm vào database
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new { productId = product.Id, fileName = product.Image });
        }


        [HttpPut("updateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductRequestDTO model, IFormFile? file)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            product.CategoryId = model.CategoryId;
            product.Name = model.Name;
            product.Status = model.Status;
            product.Description = model.Description;
            product.Specifications = model.Specifications;
            product.ProductPrice = model.ProductPrice;
            product.BasePrice = model.BasePrice;

            if (file != null && file.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "Invalid file type. Allowed types are .jpg, .jpeg, .png" });
                }

                var fileName = Guid.NewGuid() + extension;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/product", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                product.Image = fileName;
            }
            await _context.SaveChangesAsync();

            // Trả về thông tin chi tiết sản phẩm đã cập nhật
            return Ok(new
            {
                message = "Product updated successfully.",
                productId = product.Id,
                name = product.Name,
                categoryId = product.CategoryId,
                description = product.Description,
                status = product.Status,
                image = product.Image,
                price = product.Price,
                baseprice = product.BasePrice,
            });
        }


        [HttpDelete("deleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // Tìm sản phẩm theo ID
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // Xác định đường dẫn của ảnh để xóa
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/product", product.Image);

            // Kiểm tra và xóa ảnh nếu tồn tại
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            // Xóa sản phẩm khỏi database
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok("Product deleted successfully.");
        }



        [HttpGet("GetProductById/{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            // Tìm sản phẩm theo ID, bao gồm thông tin liên quan
            var product = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .Select(p => new ProductReturnDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    Image = p.Image,
                    SaleQuantity = p.SaleQuantity,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Specifications = p.Specifications,
                    ProductPrice = p.ProductPrice,
                })
                .FirstOrDefaultAsync();

            // Nếu không tìm thấy sản phẩm, trả về lỗi 404
            if (product == null)
            {
                return NotFound(new { Message = "Product not found." });
            }

            // Trả về sản phẩm
            return Ok(product);
        }

        [HttpGet("GetTop10NewestProducts")]
        public async Task<IActionResult> GetTop3NewestProducts()
        {
            // Lấy 3 sản phẩm mới nhất dựa trên CreatedAt
            var newestProducts = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Select(p => new ProductReturnDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    Image = p.Image,
                    SaleQuantity = p.SaleQuantity,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Specifications = p.Specifications,
                    ProductPrice = p.ProductPrice,
                })
                .ToListAsync();

            return Ok(newestProducts);
        }


        [HttpGet("GetBestSeller")]
        public async Task<IActionResult> GetBestSeller()
        {
            //Lấy 5 sản phẩm bán chạy nhất
            var bestseller = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.SaleQuantity)
                .Take(10)
                .Select(p => new ProductReturnDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    Image = p.Image,
                    SaleQuantity = p.SaleQuantity,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Specifications = p.Specifications,
                    ProductPrice = p.ProductPrice,
                })
                .ToListAsync();
            return Ok(bestseller);
        }


        [HttpGet("GetProductByStatus")]
        public async Task<IActionResult> GetProductByStatus(
                int pageNumber = 1,
                int? pageSize = null,
                string? name = null,
                decimal minPrice = 0,
                decimal maxPrice = 0,
                int? categoryId = null
            )
        {
            int actualPageSize = pageSize ?? _paginationSettings.DefaultPageSize;

            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == 0)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                products = products.Where(p => p.Name.Contains(name));
            }
            if (minPrice > 0)
            {
                products = products.Where(p => p.ProductPrice >= minPrice);
            }
            if (maxPrice > 0)
            {
                products = products.Where(p => p.ProductPrice <= maxPrice);
            }

            // ✅ Lọc theo category cha và các category con
            if (categoryId.HasValue)
            {
                var allCategoryIds = await _context.Categories
                    .Where(c => c.Id == categoryId.Value || c.ParentId == categoryId.Value)
                    .Select(c => c.Id)
                    .ToListAsync();

                products = products.Where(p => allCategoryIds.Contains(p.CategoryId));
            }

            int totalProductCount = await products.CountAsync();
            int totalPageCount = (int)Math.Ceiling(totalProductCount / (double)actualPageSize);
            int nextPage = pageNumber + 1 > totalPageCount ? pageNumber : pageNumber + 1;
            int previousPage = pageNumber - 1 < 1 ? pageNumber : pageNumber - 1;

            var pagingResult = new PagingReturn
            {
                TotalPageCount = totalPageCount,
                CurrentPage = pageNumber,
                NextPage = nextPage,
                PreviousPage = previousPage
            };

            List<ProductReturnDTO> productWithPaging = await products
                .Skip((pageNumber - 1) * actualPageSize)
                .Take(actualPageSize)
                .Select(p => new ProductReturnDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    Image = p.Image,
                    SaleQuantity = p.SaleQuantity,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Specifications = p.Specifications,
                    ProductPrice = p.ProductPrice,
                    BasePrice = p.BasePrice,
                })
                .ToListAsync();

            var result = new
            {
                Products = productWithPaging,
                Paging = pagingResult
            };

            return Ok(result);
        }


        [HttpGet("GetRandomProducts")]
        public async Task<IActionResult> GetRandomProducts()
        {
            // Lấy danh sách sản phẩm từ cơ sở dữ liệu và sắp xếp ngẫu nhiên
            var randomProducts = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => Guid.NewGuid())  // Sắp xếp ngẫu nhiên
                .Take(5)  // Lấy ra 5 sản phẩm
                .Select(p => new ProductReturnDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    Image = p.Image,
                    SaleQuantity = p.SaleQuantity,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Specifications = p.Specifications,
                    ProductPrice = p.ProductPrice,
                })
                .ToListAsync();

            // Nếu không tìm thấy sản phẩm, trả về NotFound
            if (randomProducts == null || !randomProducts.Any())
            {
                return NotFound(new { Message = "Không tìm thấy sản phẩm ngẫu nhiên" });
            }

            // Trả về danh sách sản phẩm ngẫu nhiên
            return Ok(randomProducts);
        }

        [HttpGet("GetRandom10Products")]
        public async Task<IActionResult> GetRandom10Products()
        {
            // Lấy danh sách sản phẩm từ cơ sở dữ liệu và sắp xếp ngẫu nhiên
            var randomProducts = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => Guid.NewGuid())  // Sắp xếp ngẫu nhiên
                .Take(10)  
                .Select(p => new ProductReturnDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    Image = p.Image,
                    SaleQuantity = p.SaleQuantity,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Specifications = p.Specifications,
                    ProductPrice = p.ProductPrice,
                })
                .ToListAsync();

            // Nếu không tìm thấy sản phẩm, trả về NotFound
            if (randomProducts == null || !randomProducts.Any())
            {
                return NotFound(new { Message = "Không tìm thấy sản phẩm ngẫu nhiên" });
            }

            // Trả về danh sách sản phẩm ngẫu nhiên
            return Ok(randomProducts);
        }

        [HttpGet("GetProductByCategoryId/{categoryId}")]
        public async Task<IActionResult> GetProductByCategoryId(int categoryId)
        {
            // Lấy danh sách sản phẩm thuộc danh mục, bao gồm thông tin danh mục và chứng nhận
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .Select(p => new ProductReturnDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    Image = p.Image,
                    SaleQuantity = p.SaleQuantity,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Specifications = p.Specifications,
                    ProductPrice = p.ProductPrice,
                })
                .ToListAsync();

            // Nếu không tìm thấy sản phẩm, trả về NotFound
            if (products == null || !products.Any())
            {
                return NotFound(new { Message = $"Không tìm thấy sản phẩm với danh mục Id = {categoryId}" });
            }

            // Trả về danh sách sản phẩm
            return Ok(products);
        }

        [HttpGet("CountProducts")]
        public async Task<IActionResult> CountProducts()
        {
            // Lấy 3 sản phẩm mới nhất dựa trên CreatedAt
            var quantity = await _context.Products.CountAsync();

            return Ok(quantity);
        }


        [HttpGet("GetCSVFile")]
        public async Task<ActionResult> GetProductCSVFile()
        {
            string baseImageUrl = "https://localhost:7205/images/product/";

            var query = _context.Products
                .Include(od => od.Category)
                .AsQueryable();

            var products = await query.ToListAsync();

            StringBuilder strCSV = new StringBuilder();
            strCSV.AppendLine("STT,Tên sản phẩm,Ảnh sản phẩm,Loại sản phẩm,Trạng thái,Số lượng bán,Ngày tạo sản phẩm");

            int index = 1;
            foreach (var product in products)
            {
                string imageUrl = $"{baseImageUrl}{product.Image}";

                string statusText = product.Status == 0 ? "Đang kinh doanh" : "Ngừng bán";

                strCSV.AppendLine($"\"{index}\"," +
                                  $"\"{product.Name}\"," +
                                  $"\"{imageUrl}\"," +
                                  $"\"{product.Category.Name}\"," +
                                  $"\"{statusText}\"," +
                                  $"\"{product.SaleQuantity}\"," +
                                  $"\"{product.CreatedAt}\"");
                index++;
            }

            byte[] bom = Encoding.UTF8.GetPreamble();
            using (MemoryStream memory = new MemoryStream())
            {
                memory.Write(bom, 0, bom.Length);
                using (StreamWriter writer = new StreamWriter(memory, Encoding.UTF8, 1024, leaveOpen: true))
                {
                    await writer.WriteAsync(strCSV.ToString());
                    await writer.FlushAsync();
                }
                memory.Position = 0;
                return File(memory.ToArray(), "text/csv", "Product.csv");
            }
        }

    }
}
