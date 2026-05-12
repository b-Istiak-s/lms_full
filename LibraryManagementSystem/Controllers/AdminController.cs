// Person 3: Admin/librarian module for managing library records and reports.
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            dbContext = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalBooks = await dbContext.Books.CountAsync();
            ViewBag.AvailableBooks = await dbContext.Books.CountAsync(b => b.IsAvailable);
            ViewBag.BorrowedBooks = await dbContext.BorrowTransactions.CountAsync(b => !b.IsReturned);
            ViewBag.MemberCount = (await userManager.GetUsersInRoleAsync("Member")).Count;
            ViewBag.PendingReservations = await dbContext.Reservations.CountAsync();
            ViewBag.UnpaidFines = await dbContext.Fines.CountAsync(f => !f.IsPaid);
            ViewBag.FeedbackCount = await dbContext.Feedbacks.CountAsync();
            return View();
        }
    }
}
