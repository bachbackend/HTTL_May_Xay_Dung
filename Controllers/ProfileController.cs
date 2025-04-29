using HTTL_May_Xay_Dung.DataAccess;
using HTTL_May_Xay_Dung.DTO;
using HTTL_May_Xay_Dung.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTTL_May_Xay_Dung.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly BfhahziulzpihzqwnwfhContext _context;
        private readonly MailService _mailService;

        public ProfileController(IConfiguration configuration, BfhahziulzpihzqwnwfhContext context, MailService mailService)
        {
            _configuration = configuration;
            _context = context;
            _mailService = mailService;
        }

        [HttpGet("Profile/{id}")]
        public async Task<ActionResult<ProfileDTO>> GetUserProfile(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new ProfileDTO
                {
                    Username = u.Username,
                    Email = u.Email,
                    Phonenumber = u.Phonenumber,
                    LastLogin = u.LastLogin,
                    Role = u.Role
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
    }
}
