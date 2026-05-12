// Person 3: Admin/librarian module for managing library records and reports.
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public ReportsController(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalBooks = await dbContext.Books.CountAsync();
            var borrowed = await dbContext.BorrowTransactions.CountAsync(b => !b.IsReturned);
            var unpaidFines = await dbContext.Fines.Where(f => !f.IsPaid).SumAsync(f => (decimal?)f.Amount) ?? 0m;

            ViewBag.TotalBooks = totalBooks;
            ViewBag.Borrowed = borrowed;
            ViewBag.UnpaidFines = unpaidFines;

            return View();
        }
    }
}
