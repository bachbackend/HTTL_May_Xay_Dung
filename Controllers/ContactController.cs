using HTTL_May_Xay_Dung.DataAccess;
using HTTL_May_Xay_Dung.DTO;
using HTTL_May_Xay_Dung.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static System.Net.Mime.MediaTypeNames;

namespace HTTL_May_Xay_Dung.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly BfhahziulzpihzqwnwfhContext _context;
        private readonly PaginationSettings _paginationSettings;
        private readonly IWebHostEnvironment _environment;

        public ContactController(IConfiguration configuration, BfhahziulzpihzqwnwfhContext context, IOptions<PaginationSettings> paginationSettings, IWebHostEnvironment environment)
        {
            _context = context;
            _paginationSettings = paginationSettings.Value;
            _environment = environment;
        }

        [Authorize(Policy = "AdminAndManagerOnly")]
        [HttpGet("GetAllContact")]
        public async Task<IActionResult> GetAllContact(
            int pageNumber = 1,
            int? pageSize = null,
            int? status = null
            )
        {
            int actualPageSize = pageSize ?? _paginationSettings.DefaultPageSize;
            var contacts = _context.Contacts
                .AsQueryable();

            if (status.HasValue)
            {
                contacts = contacts.Where(p => p.Status == status.Value);
            }

            contacts = contacts.OrderByDescending(p => p.ContactDate);

            int totalContactCount = await contacts.CountAsync();


            int totalPageCount = (int)Math.Ceiling(totalContactCount / (double)actualPageSize);
            int nextPage = pageNumber + 1 > totalPageCount ? pageNumber : pageNumber + 1;
            int previousPage = pageNumber - 1 < 1 ? pageNumber : pageNumber - 1;

            var pagingResult = new PagingReturn
            {
                TotalPageCount = totalPageCount,
                CurrentPage = pageNumber,
                NextPage = nextPage,
                PreviousPage = previousPage
            };

            List<ContactReturnDTO> contactWithPaging = await contacts
                .Skip((pageNumber - 1) * actualPageSize)
                .Take(actualPageSize)
                .Select(p => new ContactReturnDTO
                {
                    Id = p.Id,
                    Username = p.Username,
                    Phonenumber = p.Phonenumber,
                    Email = p.Email,
                    Text = p.Text,
                    Status = p.Status,
                    ContactDate = p.ContactDate,
                })
            .ToListAsync();

            var result = new
            {
                Contacts = contactWithPaging,
                Paging = pagingResult
            };

            return Ok(result);
        }

        [HttpPost("addContact")]
        public async Task<IActionResult> AddArticle([FromForm] ContactRequest model)
        {

            var contact = new Contact
            {
                Username = model.Username,
                Phonenumber = model.Phonenumber,
                Email = model.Email,
                Text = model.Text,
                Status = model.Status,
                ContactDate = model.ContactDate,
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            return Ok(new { contactId = contact.Id });
        }

        [Authorize(Policy = "AdminAndManagerOnly")]
        [HttpPatch("updateContactStatus/{id}")]
        public async Task<IActionResult> UpdateContactStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var contact = await _context.Contacts.FindAsync(id);

            if (contact == null)
            {
                return NotFound(new { message = "Contact not found" });
            }

            contact.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Status updated successfully", contactId = contact.Id, newStatus = contact.Status });
        }
    }
}
