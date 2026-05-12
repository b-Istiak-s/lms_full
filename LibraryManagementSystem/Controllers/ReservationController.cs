// Person 3: Admin/librarian module for managing library records and reports.
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using System;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public ReservationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            dbContext = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var list = await dbContext.Reservations
                .Include(r => r.Book)
                    .ThenInclude(b => b.Author)
                .Include(r => r.Book)
                    .ThenInclude(b => b.Genre)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookId, string userName, DateTime? reservationDate)
        {
            var book = await dbContext.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                TempData["ReservationError"] = "User name is required.";
                return RedirectToAction("Index", "Books");
            }

            var member = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userName.Trim() || u.Email == userName.Trim());
            if (member == null || !await userManager.IsInRoleAsync(member, "Member"))
            {
                TempData["ReservationError"] = "Please select a valid member.";
                return RedirectToAction("Index", "Books");
            }

            var reservationStart = reservationDate?.Date ?? DateTime.Today;
            var activeBorrow = await dbContext.BorrowTransactions
                .Where(t => t.BookId == bookId && !t.IsReturned)
                .OrderByDescending(t => t.DueDate)
                .FirstOrDefaultAsync();

            if (activeBorrow != null && reservationStart <= activeBorrow.DueDate.Date)
            {
                TempData["ReservationError"] = $"This book is already borrowed until {activeBorrow.DueDate:MM/dd/yyyy}.";
                return RedirectToAction("Index", "Books");
            }

            var reservation = new Reservation
            {
                BookId = bookId,
                UserId = member.Id,
                ReservationDate = reservationStart
            };

            dbContext.Reservations.Add(reservation);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Books");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var res = await dbContext.Reservations.FindAsync(id);
            if (res != null)
            {
                dbContext.Reservations.Remove(res);
                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
