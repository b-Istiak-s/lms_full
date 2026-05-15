using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    // Person 4: Member feedback and rating page.
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedbackController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            await LoadBooksAsync();
            var currentUser = await _userManager.GetUserAsync(User);

            return View(new BookFeedback
            {
                MemberName = currentUser?.FullName ?? currentUser?.Email ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(BookFeedback model)
        {
            await LoadBooksAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (model.BookId <= 0)
            {
                ViewBag.Error = "Please select a book.";
                model.MemberName = currentUser.FullName ?? currentUser.Email ?? string.Empty;
                return View(model);
            }

            if (model.Rating < 1 || model.Rating > 5)
            {
                ViewBag.Error = "Rating must be between 1 and 5.";
                model.MemberName = currentUser.FullName ?? currentUser.Email ?? string.Empty;
                return View(model);
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == model.BookId);
            if (book == null)
            {
                ViewBag.Error = "The selected book could not be found.";
                model.MemberName = currentUser.FullName ?? currentUser.Email ?? string.Empty;
                return View(model);
            }

            var feedback = new Feedback
            {
                BookId = book.BookId,
                UserId = currentUser.Id,
                Rating = model.Rating,
                Comment = model.Comment?.Trim()
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Thank you for submitting your book feedback.";
            return View(new BookFeedback
            {
                MemberName = currentUser.FullName ?? currentUser.Email ?? string.Empty
            });
        }

        private async Task LoadBooksAsync()
        {
            ViewBag.AvailableBooks = await _context.Books
                .OrderBy(b => b.Title)
                .ToListAsync();
        }
    }
}
