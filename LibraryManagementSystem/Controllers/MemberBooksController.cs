// Person 4: Member-facing book browsing and book details pages.
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class MemberBooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MemberBooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? isbn)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Where(b => b.IsAvailable)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(isbn))
            {
                var cleanIsbn = isbn.Trim();
                query = query.Where(b => b.ISBN.Contains(cleanIsbn));
                ViewBag.SearchIsbn = cleanIsbn;
            }

            var availableBooks = await query
                .OrderBy(b => b.Title)
                .ToListAsync();

            return View(availableBooks);
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .FirstOrDefaultAsync(b => b.BookId == id && b.IsAvailable);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }
    }
}
