// Person 3: Admin/librarian module for managing library records and reports.
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public AdminController(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalBooks = await dbContext.Books.CountAsync();
            ViewBag.AvailableBooks = await dbContext.Books.CountAsync(b => b.IsAvailable);
            ViewBag.BorrowedBooks = await dbContext.BorrowTransactions.CountAsync(b => !b.IsReturned);
            ViewBag.MemberCount = await dbContext.Users.CountAsync();
            ViewBag.PendingReservations = await dbContext.Reservations.CountAsync();
            ViewBag.UnpaidFines = await dbContext.Fines.CountAsync(f => !f.IsPaid);
            return View();
        }
    }
}
