// Person 4: Member borrowing page and borrow history page.
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class MemberBorrowController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberBorrowController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.AvailableBooks = await _context.Books
                .Include(b => b.Author)
                .Where(b => b.IsAvailable)
                .OrderBy(b => b.Title)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Request(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.BookId == bookId && b.IsAvailable);

            if (book == null)
            {
                TempData["BorrowMessage"] = "The selected book is not available.";
                return RedirectToAction("Index", "MemberBooks");
            }

            var rule = await _context.BorrowRules.FirstOrDefaultAsync();
            var borrowDate = DateTime.Today;
            var dueDate = borrowDate.AddDays(rule?.LoanDays ?? 14);

            var transaction = new BorrowTransaction
            {
                BookId = book.BookId,
                UserId = user.Id,
                BorrowDate = borrowDate,
                DueDate = dueDate,
                IsReturned = false
            };

            book.IsAvailable = false;
            _context.BorrowTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["BorrowMessage"] = "Book borrowed successfully. It is now listed in your borrow history.";
            return RedirectToAction("History");
        }

        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var records = await _context.BorrowTransactions
                .Include(t => t.Book)
                .Include(t => t.Fine)
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.BorrowDate)
                .ToListAsync();

            return View(records);
        }
    }
}
