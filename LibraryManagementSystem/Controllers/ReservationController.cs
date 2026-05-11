// Person 3: Admin/librarian module for managing library records and reports.
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using System;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public ReservationController(ApplicationDbContext context)
        {
            dbContext = context;
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

            var reservation = new Reservation
            {
                BookId = bookId,
                UserId = userName.Trim(),
                ReservationDate = reservationDate?.Date ?? DateTime.Now.Date
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
