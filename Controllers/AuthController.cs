using HTTL_May_Xay_Dung.DataAccess;
using HTTL_May_Xay_Dung.DTO;
using HTTL_May_Xay_Dung.Extension;
using HTTL_May_Xay_Dung.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HTTL_May_Xay_Dung.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly BfhahziulzpihzqwnwfhContext _context;
        private readonly MailService _mailService;

        public AuthController(IConfiguration configuration, BfhahziulzpihzqwnwfhContext context, MailService mailService)
        {
            _configuration = configuration;
            _context = context;
            _mailService = mailService;
        }

        //tạo tài khoản cho các role 1 2 3 
        [HttpPost("Register")]
        public async Task<IActionResult> CreateAccount([FromBody] RegisterDTO registerDTO)
        {
            if (registerDTO.Role == 0)
                return BadRequest(new { message = "Không thể tạo tài khoản với role = 0." });

            var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Username == registerDTO.Username);
            if (existingUser != null)
                return BadRequest(new { message = "Username is already exists" });

            // Tạo người dùng nhưng chưa kích hoạt
            User user = new User
            {
                Username = registerDTO.Username,
                Password = registerDTO.Password.Hash(),
                Role = registerDTO.Role,
                Phonenumber = registerDTO.PhoneNumber,
                Email = registerDTO.Email,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Register successfully." });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == loginDTO.UserName);

            if (user == null)
                return BadRequest(new { message = "Sai tên đăng nhập." });

            // Kiểm tra mật khẩu (so sánh đã băm)
            if (!loginDTO.Password.Verify(user.Password))
                return BadRequest(new { message = "Sai mật khẩu." });


            user.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            // Tạo JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

            // Cập nhật Issuer và Audience trong SecurityTokenDescriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Token hết hạn sau 1 giờ
                Issuer = _configuration["Jwt:Issuer"],  // Thêm Issuer
                Audience = _configuration["Jwt:Audience"],  // Thêm Audience
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Trả về JWT token
            return Ok(new
            {
                token = tokenString,
                userId = user.Id,
                username = user.Username,
                role = user.Role
            });
        }

        [HttpPut("ChangePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] ChangePassworđTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.NewPassword) || string.IsNullOrEmpty(dto.OldPassword))
            {
                return BadRequest(new { message = "Invalid request data." });
            }

            if (dto.NewPassword == dto.OldPassword)
            {
                return BadRequest(new { message = "New password cannot equal with old password." });
            }

            var user = await _context.Users
                .Where(a => a.Email == dto.Email)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (!dto.OldPassword.Verify(user.Password))
            {
                return BadRequest("Wrong password.");
            }

            user.Password = dto.NewPassword.Hash();
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password updated successfully." });
        }




    }
}
